using System;
using System.Collections;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Rhythm.UI {
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(CanvasGroup))]
    public class ComicPanel : MonoBehaviour {
        public event Action FadeInComplete;
        private CanvasGroup _canvasGroup;
        private void Start() {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
        }
        
        private IEnumerator FadeInPanel() {
            yield return StartCoroutine(Coroutines.FadeCanvasGroup(_canvasGroup, true));
            FadeInComplete?.Invoke();
        }
        
        public void Show() {
            StartCoroutine(FadeInPanel());
        }
    }
}