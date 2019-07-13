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
        public event Action<NoteQuality, float, int> NoteHit;
        public event Action<Song> OnExecutionStarted;
        public event Action<Song> OnExecutionFinished;
        public const float NOTE_TIME = .75f;
        public const float HALF_NOTE_TIME = NOTE_TIME / 2f;
        public const float FAIL_TOLERANCE = .20f;
        private const float MAX_WAIT_FOR_SECOND_BEAT = 8 * NOTE_TIME + FAIL_TOLERANCE;

        private double _currentBeatStartTime;
        private double _lastNoteTime;
        private float _givenBeat;
        private bool _hasBeat;
        private bool _beatStarterEnabled = true;
        private double _currentBeatRunTime;
        private int _numSuccessfulBeats;
        private SongService _songService;
        private GameStateService _gameStateService;
        private readonly List<float> _currentNotes = new List<float>();
        private readonly MonoBehaviour _coroutineProvider;
        private Song _currentSong;
        private Action _currentCommandUpdate = Constants.Noop;
        private Action<float> _update = Constants.NoopFloat;
        public BeatInputService(MonoBehaviour coroutineProvider) {
            _coroutineProvider = coroutineProvider;
        }
        
        public void Initialize() {
            #if UNITY_EDITOR
            NoteHit += (quality, diff, streak) => Debug.Log("Beat hit: " + quality + " - diff: " + diff + " - streak: " + streak);
            OnBeatLost += () => Debug.Log("Beat lost...");
            OnStreakLost += () => Debug.Log("Streak lost...");
            OnExecutionStarted += song => Debug.Log("Executing song " + song.Name);
            OnExecutionFinished += song => Debug.Log("Finished executing song " + song.Name);
            #endif
            _update = BeatInputUpdate;
        }

        public void PostInitialize() {
            _songService = ServiceLocator.Get<SongService>();
            _gameStateService = ServiceLocator.Get<GameStateService>();
            _currentBeatStartTime = AudioSettings.dspTime;
            _currentBeatRunTime = 0;
            _gameStateService.GameFinished += OnGameFinished;
            _gameStateService.GameStarted += OnGameStarted;
        }

        private void OnGameStarted() {
            _update = BeatInputUpdate;
        }
        private void OnGameFinished() {
            _update = Constants.NoopFloat;
        }

        public void Destroy() {
            _currentNotes.Clear();
            _currentCommandUpdate = Constants.Noop;
            _update = Constants.NoopFloat;
            _gameStateService.GameFinished -= OnGameFinished;
            _gameStateService.GameStarted -= OnGameStarted;
        }

        public void Update(float deltaTime) {
            _update(deltaTime);
        }

        private void BeatInputUpdate(float deltaTime) {
            _currentCommandUpdate();
            _currentBeatRunTime = AudioSettings.dspTime - _currentBeatStartTime;
            bool mouseButtonDown = Input.GetMouseButtonDown(0);
            // break the streak in case the current beat was not successfully started after a full tact (MaxWaitForSecondTact)
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
            if (_hasBeat
                && _lastNoteTime > AudioSettings.dspTime + FAIL_TOLERANCE
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
            _lastNoteTime = RoundToBeatTime((float) AudioSettings.dspTime);
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
            NoteHit?.Invoke(hitNoteQuality, noteTimeDiff, _numSuccessfulBeats);
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
            float currentBeatRunTimeFloat = (float) _currentBeatRunTime;
            float targetBeatTime = RoundToBeatTime(currentBeatRunTimeFloat);
            float beatTimeDiff = targetBeatTime - currentBeatRunTimeFloat;
            _currentNotes.Add(targetBeatTime / NOTE_TIME);
            return beatTimeDiff;
        }

        private float RoundToBeatTime(float time) {
            int currentBeatNum = Mathf.RoundToInt(time / HALF_NOTE_TIME);
            return currentBeatNum * HALF_NOTE_TIME;
        } 

        private void StartBeat() {
            
            _currentBeatStartTime = RoundToBeatTime((float) AudioSettings.dspTime);
            _currentBeatRunTime = 0;
            _currentNotes.Add(0);
        }

        private void ExecuteSong(NoteQuality hitNoteQuality, float beatTimeDiff, Song matchingSong) {
            _numSuccessfulBeats++;
            NoteHit?.Invoke(hitNoteQuality, beatTimeDiff, _numSuccessfulBeats);
            _currentSong = matchingSong;
            _currentSong.ExecuteCommand(hitNoteQuality, _numSuccessfulBeats);
            OnExecutionStarted?.Invoke(_currentSong);
            _currentCommandUpdate = _currentSong.ExecuteCommandUpdate;
            ResetBeatAfterSeconds(NOTE_TIME * 4);
        }
    }
}