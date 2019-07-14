using System;
using System.Collections.Generic;
using Rhythm.Songs;
using Rhythm.Utils;
using UnityEngine;

namespace Rhythm.Services {
    [Serializable]
    public class BeatInputService : IUpdateableService {
        public event Action OnBeatLost;
        public event Action OnStreakLost;
        public event Action<NoteQuality, float, int> OnNoteHit;
        public event Action<Song> OnExecutionStarted;
        public event Action<Song> OnExecutionFinished;
        public event Action OnMetronomeTick;
        public const float NOTE_TIME = .75f;
        public const float HALF_NOTE_TIME = NOTE_TIME / 2f;
        public const float QUARTER_NOTE_TIME = NOTE_TIME / 4f;
        public const float FAIL_TOLERANCE = .10f;
        private const float MAX_WAIT_FOR_SECOND_BEAT = 8 * NOTE_TIME + FAIL_TOLERANCE;
        private const float INDICATOR_TOLERANCE = .02f;
        private const float INDICATOR_WAIT_TIME = HALF_NOTE_TIME - INDICATOR_TOLERANCE;

        private double _lastMetronomeTickTime;
        private double _lastMetronomeTickAbsolute;
        private double _currentBeatStartTimeAbs;
        private double _lastNoteTimeAbs;
        private float _givenBeat;
        private bool _hasBeat;
        private bool _beatStarterEnabled = true;
        private bool _tickMetronome = true;
        private double _currentBeatRunTime;
        private int _numSuccessfulBeats;
        private SongService _songService;
        private GameStateService _gameStateService;
        private readonly List<float> _currentNotes = new List<float>();
        private readonly MonoBehaviour _coroutineProvider;
        private Song _currentSong;
        private Action _currentCommandUpdate = Constants.Noop;
        private Action<float> _update = Constants.NoopFloat;
        private Action _fixedUpdate = Constants.Noop;
        private Coroutine _metronomeCoroutine;
        public BeatInputService(MonoBehaviour coroutineProvider) {
            _coroutineProvider = coroutineProvider;
        }
        
        public void Initialize() {
            #if UNITY_EDITOR
            OnNoteHit += (quality, diff, streak) => Debug.Log("Beat hit: " + quality + " - diff: " + diff + " - streak: " + streak);
            OnBeatLost += () => Debug.Log("Beat lost...");
            OnStreakLost += () => Debug.Log("Streak lost...");
            OnExecutionStarted += song => Debug.Log("Executing song " + song.Name);
            OnExecutionFinished += song => Debug.Log("Finished executing song " + song.Name);
            #endif
            // _update = BeatInputUpdate;
        }

        public void PostInitialize() {
            _songService = ServiceLocator.Get<SongService>();
            _gameStateService = ServiceLocator.Get<GameStateService>();
            _currentBeatStartTimeAbs = (float) AudioSettings.dspTime;
            _currentBeatRunTime = 0;
            _gameStateService.GameFinished += Cleanup;
            _gameStateService.GameStarted += OnGameStarted;
        }

        private void OnGameStarted() {
            _update = BeatInputUpdate;
            _fixedUpdate = MetronomeTickUpdate;
        }
        private void Cleanup() {
            _currentCommandUpdate = Constants.Noop;
            _update = Constants.NoopFloat;
            _fixedUpdate = Constants.Noop;
            _currentNotes.Clear();
            _coroutineProvider.StopAllCoroutines();
            _currentBeatRunTime = 0;
        }

        public void Destroy() {
            Cleanup();
            _gameStateService.GameFinished -= Cleanup;
            _gameStateService.GameStarted -= OnGameStarted;
        }

        public void Update(float deltaTime) {
            _update(deltaTime);
        }

        public void FixedUpdate() {
            _fixedUpdate();
        }

        private void MetronomeTickUpdate() {
// metronome should kick in a tiny bit earlier than you'd expect
            if (AudioSettings.dspTime - _lastMetronomeTickTime >= INDICATOR_WAIT_TIME
                && AudioSettings.dspTime - _lastMetronomeTickAbsolute >= QUARTER_NOTE_TIME) {
                if (_tickMetronome) {
                    OnMetronomeTick?.Invoke();
                }

                _tickMetronome = !_tickMetronome;
                _lastMetronomeTickTime = _currentBeatStartTimeAbs + RoundToBeatTime((float) AudioSettings.dspTime);
                _lastMetronomeTickAbsolute = AudioSettings.dspTime;
            }
        }

        private void BeatInputUpdate(float deltaTime) {
            _currentCommandUpdate();
            _currentBeatRunTime = AudioSettings.dspTime - _currentBeatStartTimeAbs;
            
            bool mouseButtonDown = Input.GetMouseButtonDown(0);
            // break the streak in case the current beat was not successfully started after a full tact (MaxWaitForSecondBeat)
            // break the streak if a drum was hit while a song is executing
            if (_hasBeat && _currentBeatRunTime >= MAX_WAIT_FOR_SECOND_BEAT && _beatStarterEnabled
                || mouseButtonDown && _currentSong != null) {
                Debug.LogFormat("Streak lost. Details: _hasBeat: {0}, " +
                                "_currentBeatRunTime: {1} vs. max: {2}" +
                                "_beatStarterEnabled: {3}" +
                                "mouseButtonDown: {4}" +
                                "_currentSong is null: {5}", 
                    _hasBeat,
                    _currentBeatRunTime,
                    MAX_WAIT_FOR_SECOND_BEAT,
                    _beatStarterEnabled,
                    mouseButtonDown,
                    _currentSong == null);
                HandleBeatLost(true);
                return;
            }

            // break the beat if there was no hit after the max beat time
            if (!mouseButtonDown && _hasBeat
                && AudioSettings.dspTime > _lastNoteTimeAbs + NOTE_TIME + FAIL_TOLERANCE
                && _currentBeatRunTime < 4 * NOTE_TIME + FAIL_TOLERANCE) {
                HandleBeatLost();
                return;
            }

            if (mouseButtonDown && _beatStarterEnabled) {
                HandleNote();
            }
        }

        private void HandleBeatLost(bool justLoseStreak = false) {
            if (justLoseStreak) {
                OnStreakLost?.Invoke();
            } else {
                OnBeatLost?.Invoke();
            }

            _numSuccessfulBeats = 0;
            _hasBeat = false;
            ResetBeatAfterSeconds(1);
        }

        private void ResetBeatAfterSeconds(float time) {
            _currentNotes.Clear();
            _beatStarterEnabled = false;
            _coroutineProvider.StartCoroutine(Coroutines.ExecuteAfterSeconds(time, () => {
                if (_currentSong != null) {
                    _currentSong.FinishCommandExecution();
                    OnExecutionFinished?.Invoke(_currentSong);
                }

                _currentCommandUpdate = Constants.Noop;
                _currentSong = null;
                _beatStarterEnabled = true;
            }));
        }

        private void HandleNote() {
            NoteQuality hitNoteQuality;
            float noteTimeDiff = 0;
            _lastNoteTimeAbs = RoundToMetronome(AudioSettings.dspTime);
            if (!_hasBeat || _currentBeatRunTime >= 8 * NOTE_TIME - FAIL_TOLERANCE) {
                StartBeat();
                hitNoteQuality = NoteQuality.Start;
            } else {
                noteTimeDiff = CalcBeatDiff();
                hitNoteQuality = CalcNoteQuality(noteTimeDiff);
            }

            _hasBeat = true;
            float[] notesAsArray = _currentNotes.ToArray();
            List<Song> matchingSongs = _songService.CheckSongs(notesAsArray);
            
            if (matchingSongs.Count == 1 && matchingSongs[0].Matches(notesAsArray)) {
                ExecuteSong(hitNoteQuality, noteTimeDiff, matchingSongs[0]);
                return;
            } 
            if (matchingSongs.Count == 0) {
                hitNoteQuality = NoteQuality.Miss;
                Debug.Log("No songs detected with that beat! Current beat was " + string.Join("-", _currentNotes));
            }
            OnNoteHit?.Invoke(hitNoteQuality, noteTimeDiff, _numSuccessfulBeats);
            if (hitNoteQuality == NoteQuality.Miss) {
                HandleBeatLost();
            }
        }

        private static NoteQuality CalcNoteQuality(float beatTimeDiff) {
            float absBeatTimeDiff = Mathf.Abs(beatTimeDiff);

            if (absBeatTimeDiff < FAIL_TOLERANCE * .15f) {
                return NoteQuality.Perfect;
            }

            if (absBeatTimeDiff < FAIL_TOLERANCE * .5f) {
                return NoteQuality.Good;
            }

            if (absBeatTimeDiff < FAIL_TOLERANCE) {
                return NoteQuality.Bad;
            }

            return NoteQuality.Miss;
        }

        private float CalcBeatDiff() {
            double currentBeatRunTimeFloat = _currentBeatRunTime;
            double targetBeatTime = RoundToMetronome(currentBeatRunTimeFloat);
            double beatTimeDiff = targetBeatTime - currentBeatRunTimeFloat;
            _currentNotes.Add((float)targetBeatTime / NOTE_TIME);
            return (float) beatTimeDiff;
        }

        private float RoundToBeatTime(float time) {
            int currentBeatNum = Mathf.RoundToInt((float)(time - _currentBeatStartTimeAbs) / HALF_NOTE_TIME);
            return currentBeatNum * HALF_NOTE_TIME;
        }

        private double RoundToMetronome(double time) {
            int metronomeTick = Mathf.RoundToInt((float)(time - _lastMetronomeTickTime) / HALF_NOTE_TIME);
            return _lastMetronomeTickTime + metronomeTick * HALF_NOTE_TIME;
        }

        private void StartBeat() {
            _currentBeatStartTimeAbs = RoundToMetronome(AudioSettings.dspTime);
            _currentBeatRunTime = 0;
            _currentNotes.Add(0);
            Debug.Log("Starting beat at " + _currentBeatStartTimeAbs);
        }

        private void ExecuteSong(NoteQuality hitNoteQuality, float beatTimeDiff, Song matchingSong) {
            _numSuccessfulBeats++;
            OnNoteHit?.Invoke(hitNoteQuality, beatTimeDiff, _numSuccessfulBeats);
            _currentSong = matchingSong;
            _currentSong.ExecuteCommand(hitNoteQuality, _numSuccessfulBeats);
            OnExecutionStarted?.Invoke(_currentSong);
            _currentCommandUpdate = _currentSong.ExecuteCommandUpdate;
            ResetBeatAfterSeconds(NOTE_TIME * 4);
        }
    }
}