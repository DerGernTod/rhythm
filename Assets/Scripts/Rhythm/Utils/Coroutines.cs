using System;
using System.Collections;
using UnityEngine;

namespace Rhythm.Utils {
    public static class Coroutines {
        public static IEnumerator ExecuteAfterSeconds(float time, Action action) {
            float timeRemaining = time;
            while (timeRemaining > 0) {
                timeRemaining -= Time.deltaTime;
                yield return null;
            }
            action();
        }

        public static IEnumerator FadeTo(CanvasGroup canvas, float target, float time, Action onFinished = null) {
            float stepPerSec = (target - canvas.alpha) / time;
            while (Math.Abs(target - canvas.alpha) > Single.Epsilon) {
                float currentRest = target - canvas.alpha;
                float addition = stepPerSec * Time.deltaTime;
                if (Mathf.Abs(addition) > Mathf.Abs(currentRest)) {
                    addition = currentRest;
                }
                canvas.alpha += addition;
                yield return null;
            }
            canvas.alpha = target;
            onFinished?.Invoke();
        }
    }
}