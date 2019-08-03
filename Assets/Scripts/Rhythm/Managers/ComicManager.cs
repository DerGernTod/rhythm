using System;
using Rhythm.UI;
using Rhythm.Utils;
using UnityEngine;

namespace Rhythm.Managers {
    public class ComicManager : MonoBehaviour {
#pragma warning disable 0649
        [SerializeField] private SerializableEvents.UnityEventInt pageFinished;
#pragma warning restore 0649
        
        private ComicPage[] _pages;
        private int _pageIndex;
        private Action _update;
        private Coroutine _nextTimer;
        private void Start() {
            _pages = GetComponentsInChildren<ComicPage>();
            _update = ShowNextPageOnTouch;
            _nextTimer = StartCoroutine(Coroutines.ExecuteAfterSeconds(2, ShowNextPage));
        }

        private void ShowNextPageOnTouch() {
            bool mouseButtonDown = Input.GetMouseButtonDown(0);
            if (mouseButtonDown) {
                StopCoroutine(_nextTimer);
                ShowNextPage();
            }
        }

        private void ShowNextPage() {
            ComicPage curPage = _pages[_pageIndex++];
            curPage.Show();
            Action curPageOnFadeInComplete = delegate { ShowNextPanelAndStartTimer(curPage); };
            curPage.FadeInComplete += curPageOnFadeInComplete;
            curPage.AllPanelsShowComplete += () => {
                Debug.Log("All panels show complete for " + curPage.name);
                bool isLastPage = _pageIndex == _pages.Length;
                Debug.Log("Setting update to " + (isLastPage ? "noop" : "hide current page on touch"));
                _update = isLastPage ? Constants.Noop : HideCurrentPageOnTouch;
                StopCoroutine(_nextTimer);
                pageFinished?.Invoke(_pageIndex - 1);
                if (!isLastPage) {
                    _nextTimer = StartCoroutine(Coroutines.ExecuteAfterSeconds(3, LoadNextPage));
                }
            };
            Debug.Log("Setting update to show next panel on touch");
            _update = () => ShowNextPanelOnTouch(curPage);
        }

        private void ShowNextPanelOnTouch(ComicPage curPage) {
            if (Input.GetMouseButtonDown(0)) {
                ShowNextPanelAndStartTimer(curPage);
            }
        }

        private void ShowNextPanelAndStartTimer(ComicPage curPage) {
            StopCoroutine(_nextTimer);
            _nextTimer = StartCoroutine(Coroutines.ExecuteAfterSeconds(3, () => ShowNextPanelAndStartTimer(curPage)));
            curPage.ShowNextPanel();
        }

        private void HideCurrentPageOnTouch() {
            bool mouseButtonDown = Input.GetMouseButtonDown(0);
            if (mouseButtonDown) {
                LoadNextPage();
            }
        }

        private void LoadNextPage() {
            Debug.Log("Load next page called");
            ComicPage comicPage = _pages[_pageIndex - 1];
            comicPage.Hide();
            comicPage.FadeOutComplete += ShowNextPage;
            Debug.Log("Setting update to noop");
            _update = Constants.Noop;
            // stop coroutine that waits for touch to hide page, since we're now waiting on fadeoutcomplete
            StopCoroutine(_nextTimer);
        }

        private void Update() {
            _update();
        }
    }
}