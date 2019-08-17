using System;
using System.Collections;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Rhythm.UI {
    [RequireComponent(typeof(CanvasGroup))]
    public class ComicPage : MonoBehaviour {
        private CanvasGroup _canvasGroup;
        private ComicPanel[] _panels;
        private int _curPanelIndex;
        private UnityAction _destroy = Constants.Noop;
        
        public event Action AllPanelsShowComplete;
        public event Action FadeOutComplete;
        public event Action FadeInComplete;
        public bool HasMorePanels => _curPanelIndex < _panels.Length - 1;
        private void Start() {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
            _panels = GetComponentsInChildren<ComicPanel>();
        }

        private IEnumerator FadeInPage() {
            yield return StartCoroutine(Coroutines.FadeTo(_canvasGroup, 1, 1));
            FadeInComplete?.Invoke();
        }

        private IEnumerator FadeOutPage() {
            yield return StartCoroutine(Coroutines.FadeTo(_canvasGroup, 0, 1));
            FadeOutComplete?.Invoke();
        }

        public void Show() {
            StartCoroutine(FadeInPage());
        }

        public void Hide() {
            StartCoroutine(FadeOutPage());
        }

        public void ShowNextPanel() {
            if (_curPanelIndex >= _panels.Length) {
                StopAllCoroutines();
                AllPanelsShowComplete?.Invoke();
                return;
            }
            ComicPanel comicPanel = _panels[_curPanelIndex];
            comicPanel.Show();
            _curPanelIndex++;
            if (_curPanelIndex == _panels.Length) {
                Action onFadeInComplete = () => AllPanelsShowComplete?.Invoke();
                comicPanel.FadeInComplete += onFadeInComplete;
                _destroy = () => comicPanel.FadeInComplete -= onFadeInComplete;
            }
        }

        private void OnDestroy() {
            _destroy();
        }
    }
}