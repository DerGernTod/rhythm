using System;
using System.Collections.Generic;
using Rhythm;
using Services;
using UnityEngine;
using Utils;

namespace Managers {
    [Serializable]
    public class BeatInputService : IUpdateableService {
        public enum Quality {
            Miss,
            Bad,
            Good,
            Perfect,
            Start
        }
        public event Action OnBeatLost;
        public event Action<Quality> OnBeatHit;
        public const float BeatTime = .75f;
        public const float HalfBeatTime = BeatTime / 2f;
        public const float FailTolerance = .25f;
        
        private double _currentBeatStartTime;
        private double _lastBeatTime;
        private float _givenBeat;
        private bool _hasBeat;
        private bool _beatStarterEnabled = true;
        private double _currentBeatRunTime;
        private SongService _songService;
        private readonly List<float> _currentBeats = new List<float>();
        private readonly MonoBehaviour _coroutineProvider;

        public BeatInputService(MonoBehaviour coroutineProvider) {
            _coroutineProvider = coroutineProvider;
        }
        
        public void Initialize() {
            OnBeatHit += quality => Debug.Log("Beat hit: " + quality);
            OnBeatLost += () => Debug.Log("Beat lost...");
        }

        public void PostInitialize() {
            _songService = ServiceLocator.Get<SongService>();
        }

        public void Update(float deltaTime) {
            _currentBeatRunTime = AudioSettings.dspTime - _currentBeatStartTime;
            // todo: our beat should only drop if we didn't manage to follow a completed song after 4 beats
            // so store the time of our latest successful song, wait 4 beats and then compare for beat lost event
            if (_hasBeat && _currentBeatRunTime >= 4 * BeatTime + FailTolerance) {
                HandleBeatLost();
                return;
            }
            if (Input.GetMouseButtonDown(0) && _beatStarterEnabled) {
                HandleBeat();
            }
        }

        private void HandleBeatLost() {
            OnBeatLost?.Invoke();
            _hasBeat = false;
            _currentBeats.Clear();
            _beatStarterEnabled = false;
            _coroutineProvider.StartCoroutine(Coroutines.ExecuteAfterSeconds(1, () => { _beatStarterEnabled = true; }));
        }

        private void HandleBeat() {
            Quality hitQuality = Quality.Perfect;
            float currentBeatRunTimeFloat = (float) _currentBeatRunTime;
            if (!_hasBeat) {
                _currentBeatStartTime = AudioSettings.dspTime;
                _currentBeats.Add(0);
                hitQuality = Quality.Start;
            } else {
                int currentBeatNum = Mathf.RoundToInt(currentBeatRunTimeFloat / HalfBeatTime);
                float targetBeatTime = currentBeatNum * HalfBeatTime;
                float beatTimeDiff = Mathf.Abs(targetBeatTime - currentBeatRunTimeFloat);
                
                if (beatTimeDiff > FailTolerance) {
                    Debug.Log("Miss due to divergence of " + beatTimeDiff);
                    hitQuality = Quality.Miss;
                } else if (beatTimeDiff > FailTolerance * .5f) {
                    hitQuality = Quality.Bad;
                } else if (beatTimeDiff > FailTolerance * .15f) {
                    hitQuality = Quality.Good;
                }

                _currentBeats.Add(currentBeatRunTimeFloat / BeatTime);
            }

            _hasBeat = true;
            float[] beatsAsArray = _currentBeats.ToArray();
            List<Song> matchingSongs = _songService.CheckSongs(beatsAsArray);
            if (matchingSongs.Count == 0) {
                Debug.Log("Miss due to no found songs");
                hitQuality = Quality.Miss;
            } else if (matchingSongs.Count == 1 && matchingSongs[0].Matches(beatsAsArray)) {
                matchingSongs[0].ExecuteCommand();
            }

            OnBeatHit?.Invoke(hitQuality);
            if (hitQuality == Quality.Miss) {
                HandleBeatLost();
            }
        }
    }
}