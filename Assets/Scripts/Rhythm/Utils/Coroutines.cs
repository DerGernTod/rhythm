using System;
using System.Collections;
using UnityEngine;

namespace Rhythm.Utils {
    public static class Coroutines {
        public static IEnumerator ExecuteAfterSeconds(float time, Action action) {
            yield return new WaitForSeconds(time);
            action();
        }
    }
}