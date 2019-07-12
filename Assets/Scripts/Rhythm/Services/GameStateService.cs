using System;

namespace Rhythm.Services {
    public class GameStateService: IService {
        public event Action GameFinished;
        public event Action GameStarted;

        public void TriggerGameFinished() {
            GameFinished?.Invoke();
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