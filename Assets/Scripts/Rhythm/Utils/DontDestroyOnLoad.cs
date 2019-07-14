using UnityEngine;

namespace Rhythm.Utils {
    public class DontDestroyOnLoad : MonoBehaviour {
        private void Awake() {
            DontDestroyOnLoad(gameObject);
            Destroy(this);
        }
    }
}