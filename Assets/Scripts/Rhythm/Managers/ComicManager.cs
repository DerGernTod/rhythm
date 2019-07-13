using System;
using Rhythm.UI;
using Rhythm.Utils;
using UnityEngine;

namespace Rhythm.Managers {
    public class ComicManager : MonoBehaviour {
        private ComicPage[] _pages;
        private int _pageIndex;
        private Action _update;
        private void Start() {
            _pages = GetComponentsInChildren<ComicPage>();
            _update = ShowNextPageOnTouch;
        }

        private void ShowNextPageOnTouch() {
            bool mouseButtonDown = Input.GetMouseButtonDown(0);
            if (mouseButtonDown) {
                ShowNextPage();
            }
        }

        private void ShowNextPage() {
            ComicPage curPage = _pages[_pageIndex++];
            curPage.Show();
            curPage.FadeInComplete += () => curPage.ShowNextPanel();
            curPage.AllPanelsShowComplete += () => _update = HideCurrentPageOnTouch;
            _update = () => {
                if (Input.GetMouseButtonDown(0)) {
                    curPage.ShowNextPanel();
                }
            };
        }

        private void HideCurrentPageOnTouch() {
            bool mouseButtonDown = Input.GetMouseButtonDown(0);
            if (mouseButtonDown) {
                ComicPage comicPage = _pages[_pageIndex - 1];
                comicPage.Hide();
                comicPage.FadeOutComplete += ShowNextPage;
                _update = Constants.Noop;
            }
        }

        private void Update() {
            _update();
        }
    }
}