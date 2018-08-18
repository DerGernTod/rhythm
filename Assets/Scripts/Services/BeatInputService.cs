using System;
using System.Collections.Generic;
using System.Linq;
using Rhythm;
using UnityEngine;
using Utils;

namespace Services {
    [Serializable]
    public class BeatInputService : IUpdateableService {
        public event Action OnBeatLost;
        public event Action OnStreakLost;
        public event Action<BeatQuality, float> OnBeatHit;
        public event Action<Song> OnExecutionStarted;
        public event Action<Song> OnExecutionFinished;
        public const float BeatTime = .75f;
        public const float HalfBeatTime = BeatTime / 2f;
        public const float FailTolerance = .25f;
        
        private double _currentBeatStartTime;
        private double _lastBeatTime;
        private float _givenBeat;
        private float _prevBeatTimeDiff;
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
            OnBeatHit += (quality, diff) => Debug.Log("Beat hit: " + quality + " - diff: " + diff);
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

        public void Update(float deltaTime) {
            _currentUpdate();
            _currentBeatRunTime = AudioSettings.dspTime - _currentBeatStartTime;
            // todo: our beat should only drop if we didn't manage to follow a completed song after 4 beats
            // so store the time of our latest successful song, wait 4 beats and then compare for beat lost event
            bool mouseButtonDown = Input.GetMouseButtonDown(0);
            if (_hasBeat && _currentBeatRunTime >= 8 * BeatTime + FailTolerance && _beatStarterEnabled
                || mouseButtonDown && _currentSong != null) {
                Debug.LogFormat("Streak lost. Details: _hasBeat: {0}, " +
                                "_currentBeatRunTime: {1} vs. max: {2}" +
                                "_beatStarterEnabled: {3}" +
                                "mouseButtonDown: {4}" +
                                "_currentSong is null: {5}", 
                    _hasBeat,
                    _currentBeatRunTime,
                    8 * BeatTime + FailTolerance,
                    _beatStarterEnabled,
                    mouseButtonDown,
                    _currentSong == null);
                HandleBeatLost(true);
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
            BeatQuality hitBeatQuality = BeatQuality.Miss;
            float beatTimeDiff = 0;
            if (!_hasBeat || _currentBeatRunTime >= 8 * BeatTime - FailTolerance) {
                _currentBeatStartTime = AudioSettings.dspTime;
                _currentBeatRunTime = 0;
                _currentBeats.Add(0);
                _prevBeatTimeDiff = 0;
                hitBeatQuality = BeatQuality.Start;
            } else {
                float currentBeatRunTimeFloat = (float) _currentBeatRunTime;
                int currentBeatNum = Mathf.RoundToInt(currentBeatRunTimeFloat / HalfBeatTime);
                float targetBeatTime = currentBeatNum * HalfBeatTime;
                beatTimeDiff = targetBeatTime - _prevBeatTimeDiff - currentBeatRunTimeFloat;
                float absBeatTimeDiff = Mathf.Abs(beatTimeDiff);

                if (absBeatTimeDiff < FailTolerance * .15f) {
                    hitBeatQuality = BeatQuality.Perfect;
                } else if (absBeatTimeDiff < FailTolerance * .5f) {
                    hitBeatQuality = BeatQuality.Good;
                } else if (absBeatTimeDiff < FailTolerance) {
                    hitBeatQuality = BeatQuality.Bad;
                }
                _currentBeats.Add(targetBeatTime / BeatTime);
                _prevBeatTimeDiff = beatTimeDiff;
            }

            _hasBeat = true;
            float[] beatsAsArray = _currentBeats.ToArray();
            List<Song> matchingSongs = _songService.CheckSongs(beatsAsArray);
            
            if (matchingSongs.Count == 1 && matchingSongs[0].Matches(beatsAsArray)) {
                OnBeatHit?.Invoke(hitBeatQuality, beatTimeDiff);
                _currentSong = matchingSongs[0];
                _currentSong.ExecuteCommand(hitBeatQuality, _numSuccessfulTacts);
                OnExecutionStarted?.Invoke(_currentSong);
                _currentUpdate = _currentSong.ExecuteCommandUpdate;
                _numSuccessfulTacts++;
                ResetBeatAfterSeconds(BeatTime * 4);
                return;
            } 
            if (matchingSongs.Count == 0) {
                hitBeatQuality = BeatQuality.Miss;
                Debug.Log("No songs detected with that beat! Current beat was " + string.Join(",", _currentBeats));
            }
            OnBeatHit?.Invoke(hitBeatQuality, beatTimeDiff);
            if (hitBeatQuality == BeatQuality.Miss) {
                HandleBeatLost();
            }
        }
    }
}