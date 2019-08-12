using System;
using System.Collections.Generic;
using System.Linq;
using Rhythm.Songs;
using Rhythm.Utils;
using UnityEngine;

namespace Rhythm.Services {
    [Serializable]
    public class BeatInputService : IUpdateableService {
        public event Action BeatLost;
        public event Action<NoteQuality, float> NoteHit;
        public event Action<Song, int> ExecutionStarting;
        public event Action<Song, int> ExecutionStarted;
        public event Action<Song, int> ExecutionAborted;
        public event Action<Song, int> ExecutionFinishing;
        public event Action<Song, int> ExecutionFinished;
        public event Action MetronomeTick;
        public const float FAIL_TOLERANCE = .10f;
        public const float NOTE_TIME = .75f;
        public const float HALF_NOTE_TIME = NOTE_TIME / 2f;
        private const float QUARTER_NOTE_TIME = NOTE_TIME / 4f;
        private const float FAIL_TOLERANCE_GOOD = FAIL_TOLERANCE * .5f;
        private const float FAIL_TOLERANCE_PERFECT = FAIL_TOLERANCE * .15f;
        private const float MAX_WAIT_FOR_SECOND_BEAT = 8f * NOTE_TIME + FAIL_TOLERANCE;
        private const float INDICATOR_TOLERANCE = .02f;
        private const float INDICATOR_WAIT_TIME = HALF_NOTE_TIME - INDICATOR_TOLERANCE;

        public float MetronomeDiff { get; private set; }
        public bool HasBeat { get; private set; }
        public List<float> CurrentNotes => _currentNotes;

        private double _lastMetronomeTickTime;
        private double _lastMetronomeFullTickTime;
        private double _lastMetronomeTickAbsolute;
        private double _currentBeatStartTimeAbs;
        private double _lastNoteTimeAbs;
        private bool _beatStarterEnabled = true;
        private bool _tickMetronome = true;
        private double _currentBeatRunTime;
        private int _streakPower;
        private int _streakScore;
        private SongService _songService;
        private GameStateService _gameStateService;
        private readonly List<float> _currentNotes = new List<float>();
        private readonly List<NoteQuality> _currentQualities = new List<NoteQuality>();
        private readonly MonoBehaviour _coroutineProvider;
        private Song _currentSong;
        private Action _currentCommandUpdate = Constants.Noop;
        private Action<float> _update = Constants.NoopFloat;
        private Action _fixedUpdate = Constants.Noop;
        private Action _beatInputHandler = Constants.Noop;
        
        public BeatInputService(MonoBehaviour coroutineProvider) {
            _coroutineProvider = coroutineProvider;
        }
        
        public void Initialize() {
            #if UNITY_EDITOR
/*            NoteHit += (quality, diff, streak) => Debug.Log("Beat hit: " + quality + " - diff: " + diff + " - streak: " + streak);
            BeatLost += () => Debug.Log("Beat lost...");
            ExecutionStarting += song => Debug.Log("Executing song " + song.Name);
            ExecutionFinishing += song => Debug.Log("Finished executing song " + song.Name);*/
            #endif
            // _update = BeatInputUpdate;
        }

        public void PostInitialize() {
            _songService = ServiceLocator.Get<SongService>();
            _gameStateService = ServiceLocator.Get<GameStateService>();
            _currentBeatStartTimeAbs = (float) AudioSettings.dspTime;
            _currentBeatRunTime = 0;
            _gameStateService.GameFinishing += Cleanup;
            _gameStateService.GameStarted += OnGameStarted;
        }

        private void OnGameStarted() {
            double dspTime = AudioSettings.dspTime;
            _lastMetronomeTickTime = dspTime;
            _lastMetronomeFullTickTime = dspTime;
            _lastMetronomeTickAbsolute = dspTime;
            _update = BeatInputUpdate;
            _fixedUpdate = MetronomeTickUpdate;
            _beatInputHandler = HandleTouchDown;
        }
        private void Cleanup() {
            _currentCommandUpdate = Constants.Noop;
            _update = Constants.NoopFloat;
            _fixedUpdate = Constants.Noop;
            _currentNotes.Clear();
            _currentQualities.Clear();
            _coroutineProvider.StopAllCoroutines();
            _currentBeatRunTime = 0;
            _currentBeatStartTimeAbs = 0;
            _lastNoteTimeAbs = 0;
            _beatStarterEnabled = true;
            _tickMetronome = true;
            _streakPower = 0;
            _streakScore = 0;
            _currentSong = null;
            _currentCommandUpdate = Constants.Noop;
            _update = Constants.NoopFloat;
            _fixedUpdate = Constants.Noop;
            _beatInputHandler = Constants.Noop;
            HasBeat = false;
        }

        public void Destroy() {
            Cleanup();
            _gameStateService.GameFinishing -= Cleanup;
            _gameStateService.GameStarted -= OnGameStarted;
        }

        public void Update(float deltaTime) {
            _update(deltaTime);
        }

        public void FixedUpdate() {
            _fixedUpdate();
            
        }

        private void MetronomeTickUpdate() {
            double dspTime = AudioSettings.dspTime;
            // MetronomeDiff = CalcMetronomeDiffSinceLastTick() + HALF_NOTE_TIME - INDICATOR_TOLERANCE;
            MetronomeDiff = (float)(RoundToMetronomeFull(dspTime) - dspTime);
            // metronome should kick in a tiny bit earlier than you'd expect (INDICATOR_WAIT_TIME)
            if (dspTime - _lastMetronomeTickTime >= INDICATOR_WAIT_TIME
                && dspTime - _lastMetronomeTickAbsolute >= QUARTER_NOTE_TIME) {
                if (_tickMetronome) {
                    _lastMetronomeFullTickTime = RoundToMetronome(dspTime);
                    MetronomeTick?.Invoke();
                }

                _tickMetronome = !_tickMetronome;
                _lastMetronomeTickTime = RoundToMetronome(dspTime);
                // double prevTick = _lastMetronomeTickAbsolute;
                _lastMetronomeTickAbsolute = dspTime;
                // Debug.Log("Metronome tick at " + _lastMetronomeTickAbsolute + ", " + (_lastMetronomeTickAbsolute - prevTick));
            }
        }

        public void EnableTouchHandler() {
            _beatInputHandler = HandleTouchDown;
        }

        public void DisableTouchHandler() {
            _beatInputHandler = Constants.Noop;
        }

        private void BeatInputUpdate(float deltaTime) {
            _currentCommandUpdate();
            _currentBeatRunTime = AudioSettings.dspTime - _currentBeatStartTimeAbs;

            _beatInputHandler();
        }

        private void HandleTouchDown() {
            bool mouseButtonDown = Input.GetMouseButtonDown(0);
            // break the streak in case the current beat was not successfully started after a full tact (MaxWaitForSecondBeat)
            // break the streak if a drum was hit while a song is executing
            if (HasBeat && _currentBeatRunTime >= MAX_WAIT_FOR_SECOND_BEAT && _beatStarterEnabled
                || mouseButtonDown && _currentSong != null) {
                Debug.LogFormat("Streak lost. Details: _hasBeat: {0}, " +
                                "_currentBeatRunTime: {1} vs. max: {2}" +
                                "_beatStarterEnabled: {3}" +
                                "mouseButtonDown: {4}" +
                                "_currentSong is null: {5}",
                    HasBeat,
                    _currentBeatRunTime,
                    MAX_WAIT_FOR_SECOND_BEAT,
                    _beatStarterEnabled,
                    mouseButtonDown,
                    _currentSong == null);
                HandleBeatLost();
                return;
            }

            // break the beat if there was no hit after the max beat time
            if (!mouseButtonDown && HasBeat
                                 && AudioSettings.dspTime > _lastNoteTimeAbs + NOTE_TIME + FAIL_TOLERANCE
                                 && _currentBeatRunTime < 4 * NOTE_TIME + FAIL_TOLERANCE) {
                HandleBeatLost();
                return;
            }

            if (mouseButtonDown && _beatStarterEnabled) {
                HandleNoteHit();
            }
        }

        private void HandleBeatLost() {
            BeatLost?.Invoke();
            if (_currentSong != null) {
                ExecutionAborted?.Invoke(_currentSong, 0);
                _currentSong = null;
            }
            _streakPower = 0;
            _streakScore = 0;
            HasBeat = false;
            ResetBeatAfterSeconds(1);
        }

        private void ResetBeatAfterSeconds(float time) {
            _currentNotes.Clear();
            _currentQualities.Clear();
            _beatStarterEnabled = false;
            _coroutineProvider.StartCoroutine(Coroutines.ExecuteAfterSeconds(time, () => {
                if (_currentSong != null) {
                    _currentSong.FinishCommandExecution();
                    ExecutionFinishing?.Invoke(_currentSong, _streakPower);
                    Song activeSong = _currentSong;
                    _coroutineProvider.StartCoroutine(Coroutines.ExecuteAfterSeconds(HALF_NOTE_TIME, () => {
                        ExecutionFinished?.Invoke(activeSong, _streakPower);
                        EnableTouchHandler();
                    }));
                }

                _currentCommandUpdate = Constants.Noop;
                _currentSong = null;
                _beatStarterEnabled = true;
            }));
        }

        private void HandleNoteHit() {
            double dspTime = AudioSettings.dspTime;
            double closestHalfNote = RoundToMetronome(dspTime);
            double closestFullNote = RoundToMetronomeFull(dspTime);
            _lastNoteTimeAbs = closestHalfNote;
            float noteTimeDiff = (float)(_lastNoteTimeAbs - dspTime);
            float noteTimeDiffToFull = (float) (closestFullNote - dspTime);
            bool canCreateBeat = !HasBeat || _currentBeatRunTime >= 8 * NOTE_TIME - FAIL_TOLERANCE;
            
            if (canCreateBeat && Mathf.Abs(noteTimeDiffToFull) <= FAIL_TOLERANCE) {
                _currentBeatRunTime = noteTimeDiffToFull;
                _currentBeatStartTimeAbs = RoundToMetronome(dspTime);
            }
            NoteQuality hitNoteQuality = CalcNoteQuality(noteTimeDiff);

            if (hitNoteQuality != NoteQuality.Miss) {
                _currentNotes.Add(Mathf.RoundToInt((float) _currentBeatRunTime / HALF_NOTE_TIME) * .5f);
                _currentQualities.Add(hitNoteQuality);
            }

            HasBeat = true;
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
            NoteHit?.Invoke(hitNoteQuality, noteTimeDiff);
            if (hitNoteQuality == NoteQuality.Miss) {
                HandleBeatLost();
            }
        }

        public static NoteQuality CalcNoteQuality(float beatTimeDiff) {
            float absBeatTimeDiff = Mathf.Abs(beatTimeDiff);

            if (absBeatTimeDiff < FAIL_TOLERANCE_PERFECT) {
                return NoteQuality.Perfect;
            }

            if (absBeatTimeDiff < FAIL_TOLERANCE_GOOD) {
                return NoteQuality.Good;
            }

            if (absBeatTimeDiff < FAIL_TOLERANCE) {
                return NoteQuality.Bad;
            }

            return NoteQuality.Miss;
        }
        
        private double RoundToMetronome(double time) {
            int metronomeTick = Mathf.RoundToInt((float)(time - _lastMetronomeTickTime) / HALF_NOTE_TIME);
            return _lastMetronomeTickTime + metronomeTick * HALF_NOTE_TIME;
        }
        private double RoundToMetronomeFull(double time) {
            int metronomeTick = Mathf.RoundToInt((float)(time - _lastMetronomeFullTickTime) / NOTE_TIME);
            return _lastMetronomeFullTickTime + metronomeTick * NOTE_TIME;
        }

        private void ExecuteSong(NoteQuality hitNoteQuality, float beatTimeDiff, Song matchingSong) {
            NoteHit?.Invoke(hitNoteQuality, beatTimeDiff);
            if (_beatInputHandler == Constants.Noop) {
                // don't execute song if game has been finished with the last note
                return;
            }
            _streakScore += _currentQualities.Aggregate(0, (total, curQuality) => total + (int) curQuality);
            _streakPower = Mathf.Min(_streakScore / Constants.REQUIRED_STREAK_SCORE, Constants.MAX_STREAK_POWER);
            _currentSong = matchingSong;
            _currentSong.ExecuteCommand(hitNoteQuality, _streakPower);
            _coroutineProvider.StartCoroutine(Coroutines.ExecuteAfterSeconds(HALF_NOTE_TIME, () => {
                ExecutionStarted?.Invoke(matchingSong, _streakPower);
                DisableTouchHandler();
            }));
            ExecutionStarting?.Invoke(_currentSong, _streakPower);
            _currentCommandUpdate = _currentSong.ExecuteCommandUpdate;
            ResetBeatAfterSeconds(NOTE_TIME * 4);
        }
    }
}