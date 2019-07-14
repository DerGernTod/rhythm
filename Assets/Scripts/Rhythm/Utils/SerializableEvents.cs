using System;
using UnityEngine.Events;

namespace Rhythm.Utils {
    public static class SerializableEvents {
        [Serializable]
        public class UnityEventInt : UnityEvent<int> { }
    }
}