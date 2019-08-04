using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

        public static IEnumerator FadeColor(Image image, Color target, float time, Action onFinished = null) {
            Color from = image.color;
            float curTime = 0;
            while (curTime < time) {
                curTime += Time.deltaTime;
                image.color = Color.Lerp(from, target, curTime / time);
                yield return null;
            }

            image.color = target;
            onFinished?.Invoke();
        }
        

        public static IEnumerator FadeTo(CanvasGroup canvas, float target, float time, Action onFinished = null) {
            float from = canvas.alpha;
            float curTime = 0;
            while (curTime < time) {
                curTime += Time.deltaTime;
                canvas.alpha = Mathf.Lerp(from, target, curTime / time);
                yield return null;
            }

            canvas.alpha = target;
            onFinished?.Invoke();
        }
    }
}