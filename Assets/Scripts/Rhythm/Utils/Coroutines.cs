using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
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

        public static IEnumerator MoveAlongCurve(Transform transform, AnimationCurve curve, Vector3 axis, float time, bool useWorldSpace = true, UnityAction onComplete = null) {
            float curTime = 0;
            Vector3 prevMovePos = axis * curve.Evaluate(0);
            while (curTime < time) {
                curTime += Time.deltaTime;
                Vector3 curMovePos = axis * curve.Evaluate(curTime / time);
                Vector3 diff = curMovePos - prevMovePos;
                if (useWorldSpace) {
                    transform.position += diff;
                } else {
                    transform.localPosition += diff;
                }

                prevMovePos = curMovePos;
                yield return null;
            }
            Vector3 finalDiff = axis * curve.Evaluate(1) - prevMovePos;
            if (useWorldSpace) {
                transform.position += finalDiff;
            } else {
                transform.localPosition += finalDiff;
            }
            onComplete?.Invoke();
        }

        public static IEnumerator FadeColor(GameObject image, Color target, float time, UnityAction onComplete = null) {
            SpriteRenderer spriteRenderer = image.GetComponent<SpriteRenderer>();
            Image unityImage = image.GetComponent<Image>();
            Color from = spriteRenderer ? spriteRenderer.color : unityImage.color;
            float curTime = 0;
            while (curTime < time) {
                curTime += Time.deltaTime;
                Color targetColor = Color.Lerp(from, target, curTime / time);
                if (spriteRenderer) {
                    spriteRenderer.color = targetColor;
                } else {
                    unityImage.color = targetColor;
                }
                yield return null;
            }

            if (spriteRenderer) {
                spriteRenderer.color = target;
            } else {
                unityImage.color = target;
            }
            onComplete?.Invoke();
        }
        
        
        

        public static IEnumerator FadeTo(CanvasGroup canvas, float target, float time, UnityAction onComplete = null) {
            float from = canvas.alpha;
            float curTime = 0;
            while (curTime < time) {
                curTime += Time.deltaTime;
                canvas.alpha = Mathf.Lerp(from, target, curTime / time);
                yield return null;
            }

            canvas.alpha = target;
            onComplete?.Invoke();
        }
    }
}