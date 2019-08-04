using System;
using Rhythm.Utils;

namespace Rhythm.Services {
    public class GameStateService: IService {
        public event Action GameFinishing;
        public event Action GameStarted;
        public event Action<BuildScenes?, BuildScenes> SceneTransitionStarted;
        public event Action<BuildScenes?, BuildScenes> SceneTransitionFinished;

        private BuildScenes? _buildSceneFrom;
        private BuildScenes _buildSceneTo;
        private BuildScenes? _currentBuildScene;

        public void TriggerSceneTransition(BuildScenes to) {
            _buildSceneFrom = _currentBuildScene;
            _buildSceneTo = to;
            SceneTransitionStarted?.Invoke(_buildSceneFrom, _buildSceneTo);
        }

        public void TriggerSceneTransitionFinished() {
            SceneTransitionFinished?.Invoke(_buildSceneFrom, _buildSceneTo);
            _currentBuildScene = _buildSceneTo;
            _buildSceneFrom = null;
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