using System;
using Rhythm.Services;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rhythm.UI {
    [RequireComponent(typeof(CanvasGroup))]
    public class SceneTransitionOverlay : MonoBehaviour {
#pragma warning disable 0649
        [SerializeField] private float transitionTime = 1.5f;
#pragma warning restore 0649
        
        private GameStateService _gameStateService;
        private CanvasGroup _canvasGroup;

        private void Start() {
            _gameStateService = ServiceLocator.Get<GameStateService>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _gameStateService.SceneTransitionStarted += OnSceneTransitionStarted;
        }

        private void OnDestroy() {
            _gameStateService.SceneTransitionStarted -= OnSceneTransitionStarted;
        }

        private void OnSceneTransitionStarted(string from, string to) {
            StartCoroutine(Coroutines.FadeTo(_canvasGroup, 1, transitionTime, () => {
                SceneManager.LoadSceneAsync(to).completed +=
                    operation => StartCoroutine(
                        Coroutines.FadeTo(_canvasGroup, 0, transitionTime, () => {
                        _gameStateService.TriggerSceneTransitionFinished();
                    }));
            }));
        }
    }
}