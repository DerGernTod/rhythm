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

        private void Awake() {
            _gameStateService = ServiceLocator.Get<GameStateService>();
            _gameStateService.SceneTransitionStarted += OnSceneTransitionStarted;
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnDestroy() {
            _gameStateService.SceneTransitionStarted -= OnSceneTransitionStarted;
        }

        private void OnSceneTransitionStarted(BuildScenes? from, BuildScenes to) {
            StartCoroutine(Coroutines.FadeTo(_canvasGroup, 1, transitionTime, () => {
                SceneManager.LoadSceneAsync((int)to).completed +=
                    operation => StartCoroutine(
                        Coroutines.FadeTo(_canvasGroup, 0, transitionTime, () => {
                        _gameStateService.TriggerSceneTransitionFinished();
                    }));
            }));
        }
    }
}