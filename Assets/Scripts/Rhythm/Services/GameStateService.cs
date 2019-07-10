using System;

namespace Rhythm.Services {
    public class GameStateService: IService {
        public event Action GameFinished;

        public void TriggerGameFinished() {
            GameFinished?.Invoke();
        }
        public void Initialize() {
        }

        public void PostInitialize() {
        }

        public void Destroy() {
        }
    }
}