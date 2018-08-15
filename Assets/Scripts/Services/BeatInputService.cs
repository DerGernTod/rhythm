using System;
using System.Collections.Generic;
using Rhythm;
using UnityEngine;
using Utils;

namespace Services {
    [Serializable]
    public class BeatInputService : IUpdateableService {
        public event Action OnBeatLost;
        public event Action<BeatQuality, float> OnBeatHit;
        public event Action<Song, BeatQuality> OnSongComplete;
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
        private bool _isExecutingCommands;

        public BeatInputService(MonoBehaviour coroutineProvider) {
            _coroutineProvider = coroutineProvider;
        }
        
        public void Initialize() {
            // this is just for demo/debug
            OnBeatHit += (quality, diff) => Debug.Log("Beat hit: " + quality + " - diff: " + diff);
            OnBeatLost += () => Debug.Log("Beat lost...");
            OnSongComplete += (song, quality) => song.ExecuteCommand(quality);
        }

        public void PostInitialize() {
            _songService = ServiceLocator.Get<SongService>();
            _currentBeatStartTime = AudioSettings.dspTime;
            _currentBeatRunTime = 0;
        }

        public void Update(float deltaTime) {
            _currentBeatRunTime = AudioSettings.dspTime - _currentBeatStartTime;
            // todo: our beat should only drop if we didn't manage to follow a completed song after 4 beats
            // so store the time of our latest successful song, wait 4 beats and then compare for beat lost event
            bool mouseButtonDown = Input.GetMouseButtonDown(0);
            if ((_hasBeat && _currentBeatRunTime >= 8 * BeatTime + FailTolerance && _beatStarterEnabled)
                || (mouseButtonDown && _isExecutingCommands)) {
                HandleBeatLost();
                return;
            }

            if (mouseButtonDown && _beatStarterEnabled) {
                HandleBeat();
            }
        }

        private void HandleBeatLost() {
            OnBeatLost?.Invoke();
            _numSuccessfulTacts = 0;
            _hasBeat = false;
            ResetBeatAfterSeconds(1);
        }

        private void ResetBeatAfterSeconds(float time) {
            _currentBeats.Clear();
            _beatStarterEnabled = false;
            _coroutineProvider.StartCoroutine(Coroutines.ExecuteAfterSeconds(time, () => {
                _isExecutingCommands = false;
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
            OnBeatHit?.Invoke(hitBeatQuality, beatTimeDiff);

            _hasBeat = true;
            float[] beatsAsArray = _currentBeats.ToArray();
            List<Song> matchingSongs = _songService.CheckSongs(beatsAsArray);
            if (matchingSongs.Count == 1 && matchingSongs[0].Matches(beatsAsArray)) {
                OnSongComplete?.Invoke(matchingSongs[0], hitBeatQuality);
                _numSuccessfulTacts++;
                Debug.Log("Num successful tacts: " + _numSuccessfulTacts);
                _isExecutingCommands = true;
                ResetBeatAfterSeconds(BeatTime * 4);
            }
            if (hitBeatQuality == BeatQuality.Miss || matchingSongs.Count != 1 && _currentBeatRunTime > BeatTime * 4) {
                if (matchingSongs.Count > 1) {
                    Debug.LogError("Detected " + matchingSongs.Count + " matching songs although song should be finished!");
                }
                HandleBeatLost();
            }
        }
    }
}