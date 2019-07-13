using System;
using System.Collections;
using Rhythm.Utils;
using UnityEngine;

namespace Rhythm.UI {
    [RequireComponent(typeof(CanvasGroup))]
    public class ComicPage : MonoBehaviour {
        private CanvasGroup _canvasGroup;
        private ComicPanel[] _panels;
        private int _curPanelIndex;
        private Action _destroy = Constants.Noop;
        
        public event Action AllPanelsShowComplete;
        public event Action FadeOutComplete;
        public event Action FadeInComplete;
        
        private void Start() {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
            _panels = GetComponentsInChildren<ComicPanel>();
        }

        private IEnumerator FadeInPage() {
            yield return StartCoroutine(Coroutines.FadeCanvasGroup(_canvasGroup, true));
            FadeInComplete?.Invoke();
        }

        private IEnumerator FadeOutPage() {
            yield return StartCoroutine(Coroutines.FadeCanvasGroup(_canvasGroup, false));
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