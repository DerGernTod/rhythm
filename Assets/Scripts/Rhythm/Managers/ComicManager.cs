using System;
using System.Collections.Generic;
using Rhythm.UI;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Rhythm.Managers {
    public class ComicManager : MonoBehaviour {
#pragma warning disable 0649
        [SerializeField] private SerializableEvents.UnityEventInt pageFinished;
#pragma warning restore 0649
        
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
            curPage.AllPanelsShowComplete += () => {
                _update = _pageIndex == _pages.Length ? Constants.Noop : HideCurrentPageOnTouch;
                pageFinished?.Invoke(_pageIndex - 1);
            };
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