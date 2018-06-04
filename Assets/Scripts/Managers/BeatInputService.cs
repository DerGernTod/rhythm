using System;
using System.Collections.Generic;
using Rhythm;
using Services;
using UnityEngine;
using Utils;

namespace Managers {
    public class BeatInputService : IUpdateableService {
        public event Action OnBeatLost; 
        public const float BeatTime = .75f;
        public const float FailTolerance = .15f;
        
        private double _currentBeatStartTime;
        private double _lastBeatTime;
        private float _givenBeat;
        private bool _hasBeat;
        private bool _beatStarterEnabled = true;
        private double _currentBeatRunTime;
        private List<float> _currentBeats = new List<float>();
        private SongService _songService;
        private MonoBehaviour _coroutineProvider;

        public BeatInputService(MonoBehaviour coroutineProvider) {
            _coroutineProvider = coroutineProvider;
        }
        
        public void Initialize() {
        }

        public void PostInitialize() {
            _songService = ServiceLocator.Get<SongService>();
        }

        public void Update(float deltaTime) {
            _currentBeatRunTime = AudioSettings.dspTime - _currentBeatStartTime;
            if (_hasBeat && _currentBeatRunTime >= 4 * BeatTime + FailTolerance) {
                OnBeatLost?.Invoke();
                _hasBeat = false;
                _currentBeats.Clear();
                _beatStarterEnabled = false;
                Debug.Log("Beat lost, resetting after 1 second");
                _coroutineProvider.StartCoroutine(Coroutines.ExecuteAfterSeconds(1, () => {
                    _beatStarterEnabled = true;
                    Debug.Log("Beatstarter enabled again!");
                }));
                return;
            }
            if (Input.GetMouseButtonDown(0) && _beatStarterEnabled) {
                if (!_hasBeat) {
                    Debug.Log("Beat started!");
                    _currentBeatStartTime = AudioSettings.dspTime;
                    _currentBeats.Add(0);
                } else {
                    _currentBeats.Add((float)_currentBeatRunTime / BeatTime);
                }
                _hasBeat = true;
                _songService.CheckSongs(_currentBeats.ToArray())?.ExecuteCommand();
            }
        }
    }
}