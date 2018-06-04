using System;
using System.Collections;
using UnityEngine;

namespace Utils {
    public class Coroutines {
        public static IEnumerator ExecuteAfterSeconds(float time, Action action) {
            yield return new WaitForSeconds(time);
            action();
        }
    }
}