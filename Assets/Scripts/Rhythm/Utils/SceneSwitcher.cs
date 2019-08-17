using System;
using Rhythm.Services;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Rhythm.Utils {
    public class SceneSwitcher : MonoBehaviour {
#pragma warning disable 0649
        [SerializeField] private Text tapToContinueText;
        [FormerlySerializedAs("targetScene")] [SerializeField] private BuildScenes targetBuildScene;
#pragma warning restore 0649
        
        private GameStateService _gameStateService;
        private UnityAction _update = Constants.Noop;
        private float _sinTime;
        private void Start() {
            _gameStateService = ServiceLocator.Get<GameStateService>();
            _gameStateService.GameFinishing += GameStateServiceOnGameFinishing;
        }

        private void OnDestroy() {
            _gameStateService.GameFinishing -= GameStateServiceOnGameFinishing;
        }

        private void GameStateServiceOnGameFinishing() {
            StartCoroutine(Coroutines.ExecuteAfterSeconds(1, () => { _update = WaitForTouch; }));
            iTween.FadeTo(tapToContinueText.gameObject, .25f, 1);
        }

        private void WaitForTouch() {
            Color textColor = tapToContinueText.color;
            textColor.a = .25f + .125f * Mathf.Sin(_sinTime);
            _sinTime += Time.deltaTime * 2;
            tapToContinueText.color = textColor;
            if (Input.GetMouseButtonDown(0)) {
                _update = Constants.Noop;
                _gameStateService.TriggerSceneTransition(targetBuildScene);
            }
        }

        private void Update() {
            _update();
        }
    }
}