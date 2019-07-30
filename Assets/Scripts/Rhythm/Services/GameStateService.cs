using System;
using UnityEngine.SceneManagement;

namespace Rhythm.Services {
    public class GameStateService: IService {
        public event Action GameFinishing;
        public event Action GameStarted;
        public event Action<string, string> SceneTransitionStarted;
        public event Action<string, string> SceneTransitionFinished;

        private string _sceneFrom;
        private string _sceneTo;

        public void TriggerSceneTransition(string to) {
            _sceneFrom = SceneManager.GetActiveScene().name;
            _sceneTo = to;
            SceneTransitionStarted?.Invoke(_sceneFrom, _sceneTo);
        }

        public void TriggerSceneTransitionFinished() {
            SceneTransitionFinished?.Invoke(_sceneFrom, _sceneTo);
            _sceneFrom = null;
            _sceneTo = null;
        }
        
        public void TriggerGameFinishing() {
            GameFinishing?.Invoke();
        }

        public void TriggerGameStarted() {
            GameStarted?.Invoke();
        }
        public void Initialize() {
        }

        public void PostInitialize() {
        }

        public void Destroy() {
        }
    }
}