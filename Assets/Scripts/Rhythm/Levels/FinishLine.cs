using Rhythm.Services;
using Rhythm.Units;
using UnityEngine;

namespace Rhythm.Levels {
    public class FinishLine : MonoBehaviour {
        private void OnTriggerEnter(Collider other) {
            if (other.GetComponent<Unit>()) {
                ServiceLocator.Get<GameStateService>().TriggerGameFinished();
            }
        }
    }
}