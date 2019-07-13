using System;
using System.Collections;
using UnityEngine;

namespace Rhythm.Utils {
    public static class Coroutines {
        public static IEnumerator ExecuteAfterSeconds(float time, Action action) {
            yield return new WaitForSeconds(time);
            action();
        }
        
        public static IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, bool fadeIn) {
            while (fadeIn ? canvasGroup.alpha < 1 : canvasGroup.alpha > 0) {
                canvasGroup.alpha += Time.deltaTime * (fadeIn ? 1 : -1);
                yield return 0;
            }
            canvasGroup.alpha = fadeIn ? 1 : 0;
        }
    }
}