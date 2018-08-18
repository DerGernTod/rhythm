using System;
using System.Collections.Generic;
using Rhythm;
using UnityEngine;
using Utils;

namespace Services {
    [Serializable]
    public class BeatInputService : IUpdateableService {
        public event Action OnBeatLost;
        public event Action OnStreakLost;
        public event Action<BeatQuality, float, int> OnBeatHit;
        public event Action<Song> OnExecutionStarted;
        public event Action<Song> OnExecutionFinished;
        public const float BeatTime = .75f;
        public const float HalfBeatTime = BeatTime / 2f;
        public const float FailTolerance = .20f;
        private const float MaxWaitForSecondTact = 8 * BeatTime + FailTolerance;

        private double _currentBeatStartTime;
        private double _lastBeatTime;
        private float _givenBeat;
        private bool _hasBeat;
        private bool _beatStarterEnabled = true;
        private double _currentBeatRunTime;
        private int _numSuccessfulTacts;
        private SongService _songService;
        private readonly List<float> _currentBeats = new List<float>();
        private readonly MonoBehaviour _coroutineProvider;
        private Song _currentSong;
        private Action _currentUpdate = Constants.Noop;
        public BeatInputService(MonoBehaviour coroutineProvider) {
            _coroutineProvider = coroutineProvider;
        }
        
        public void Initialize() {
            // this is just for demo/debug
            OnBeatHit += (quality, diff, streak) => Debug.Log("Beat hit: " + quality + " - diff: " + diff + " - streak: " + streak);
            OnBeatLost += () => Debug.Log("Beat lost...");
            OnStreakLost += () => Debug.Log("Streak lost...");
            OnExecutionStarted += song => Debug.Log("Executing song " + song.Name);
            OnExecutionFinished += song => Debug.Log("Finished executing song " + song.Name);
        }

        public void PostInitialize() {
            _songService = ServiceLocator.Get<SongService>();
            _currentBeatStartTime = AudioSettings.dspTime;
            _currentBeatRunTime = 0;
        }

        public void Destroy() {
            _currentBeats.Clear();
            _currentUpdate = Constants.Noop;
        }

        public void Update(float deltaTime) {
            _currentUpdate();
            _currentBeatRunTime = AudioSettings.dspTime - _currentBeatStartTime;
            bool mouseButtonDown = Input.GetMouseButtonDown(0);
            // break the streak in case the current beat was not successfully started after a full tact (MaxWaitForSecondTact)
            // break the streak if a drum was hit while a song is executing
            if (_hasBeat && _currentBeatRunTime >= MaxWaitForSecondTact && _beatStarterEnabled
                || mouseButtonDown && _currentSong != null) {
                Debug.LogFormat("Streak lost. Details: _hasBeat: {0}, " +
                                "_currentBeatRunTime: {1} vs. max: {2}" +
                                "_beatStarterEnabled: {3}" +
                                "mouseButtonDown: {4}" +
                                "_currentSong is null: {5}", 
                    _hasBeat,
                    _currentBeatRunTime,
                    MaxWaitForSecondTact,
                    _beatStarterEnabled,
                    mouseButtonDown,
                    _currentSong == null);
                HandleBeatLost(true);
                return;
            }

            // break the beat if there was no hit after the max beat time
            if (_hasBeat
                && _lastBeatTime > AudioSettings.dspTime + FailTolerance
                && _currentBeatRunTime < 4 * BeatTime + FailTolerance) {
                HandleBeatLost();
                return;
            }

            if (mouseButtonDown && _beatStarterEnabled) {
                HandleBeat();
            }
        }

        private void HandleBeatLost(bool justLoseStreak = false) {
            if (justLoseStreak) {
                OnStreakLost?.Invoke();
            } else {
                OnBeatLost?.Invoke();
            }

            _numSuccessfulTacts = 0;
            _hasBeat = false;
            ResetBeatAfterSeconds(1);
        }

        private void ResetBeatAfterSeconds(float time) {
            _currentBeats.Clear();
            _beatStarterEnabled = false;
            _coroutineProvider.StartCoroutine(Coroutines.ExecuteAfterSeconds(time, () => {
                if (_currentSong != null) {
                    _currentSong.FinishCommandExecution();
                    OnExecutionFinished?.Invoke(_currentSong);
                }

                _currentUpdate = Constants.Noop;
                _currentSong = null;
                _beatStarterEnabled = true;
            }));
        }

        private void HandleBeat() {
            BeatQuality hitBeatQuality;
            float beatTimeDiff = 0;
            _lastBeatTime = RoundToBeatTime((float) AudioSettings.dspTime);
            if (!_hasBeat || _currentBeatRunTime >= 8 * BeatTime - FailTolerance) {
                StartTact();
                hitBeatQuality = BeatQuality.Start;
            } else {
                beatTimeDiff = CalcBeatDiff();
                hitBeatQuality = CalcBeatQuality(beatTimeDiff);
            }

            _hasBeat = true;
            float[] beatsAsArray = _currentBeats.ToArray();
            List<Song> matchingSongs = _songService.CheckSongs(beatsAsArray);
            
            if (matchingSongs.Count == 1 && matchingSongs[0].Matches(beatsAsArray)) {
                ExecuteSong(hitBeatQuality, beatTimeDiff, matchingSongs[0]);
                return;
            } 
            if (matchingSongs.Count == 0) {
                hitBeatQuality = BeatQuality.Miss;
                Debug.Log("No songs detected with that beat! Current beat was " + string.Join(",", _currentBeats));
            }
            OnBeatHit?.Invoke(hitBeatQuality, beatTimeDiff, _numSuccessfulTacts);
            if (hitBeatQuality == BeatQuality.Miss) {
                HandleBeatLost();
            }
        }

        private static BeatQuality CalcBeatQuality(float beatTimeDiff) {
            float absBeatTimeDiff = Mathf.Abs(beatTimeDiff);

            if (absBeatTimeDiff < FailTolerance * .15f) {
                return BeatQuality.Perfect;
            }

            if (absBeatTimeDiff < FailTolerance * .5f) {
                return BeatQuality.Good;
            }

            if (absBeatTimeDiff < FailTolerance) {
                return BeatQuality.Bad;
            }

            return BeatQuality.Miss;
        }

        private float CalcBeatDiff() {
            float currentBeatRunTimeFloat = (float) _currentBeatRunTime;
            float targetBeatTime = RoundToBeatTime(currentBeatRunTimeFloat);
            float beatTimeDiff = targetBeatTime - currentBeatRunTimeFloat;
            _currentBeats.Add(targetBeatTime / BeatTime);
            return beatTimeDiff;
        }

        private float RoundToBeatTime(float time) {
            int currentBeatNum = Mathf.RoundToInt(time / HalfBeatTime);
            return currentBeatNum * HalfBeatTime;
        } 

        private void StartTact() {
            
            _currentBeatStartTime = RoundToBeatTime((float) AudioSettings.dspTime);
            _currentBeatRunTime = 0;
            _currentBeats.Add(0);
        }

        private void ExecuteSong(BeatQuality hitBeatQuality, float beatTimeDiff, Song matchingSong) {
            _numSuccessfulTacts++;
            OnBeatHit?.Invoke(hitBeatQuality, beatTimeDiff, _numSuccessfulTacts);
            _currentSong = matchingSong;
            _currentSong.ExecuteCommand(hitBeatQuality, _numSuccessfulTacts);
            OnExecutionStarted?.Invoke(_currentSong);
            _currentUpdate = _currentSong.ExecuteCommandUpdate;
            ResetBeatAfterSeconds(BeatTime * 4);
        }
    }
}