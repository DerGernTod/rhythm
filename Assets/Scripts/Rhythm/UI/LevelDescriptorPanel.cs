using System;
using Rhythm.Levels;
using Rhythm.Services;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Rhythm.UI {
    [RequireComponent(typeof(CanvasGroup))]
    public class LevelDescriptorPanel : MonoBehaviour {
#pragma warning disable 0649
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button playButton;
        [SerializeField] private GameObject detailsPanel;
        [SerializeField] private Text heading;
#pragma warning restore 0649
        private LevelData _currentLevel;
        private CanvasGroup _canvasGroup;
        private Vector3 _startPos;

        private void Start() {
            cancelButton.onClick.AddListener(Hide);
            playButton.onClick.AddListener(StartLevel);
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
            Hide();
            _startPos = transform.position;
        }

        public void SetLevel(LevelData level) {
            _currentLevel = level;
            heading.text = "Start Level " + level.name;
        }

        private void StartLevel() {
            ServiceLocator.Get<GameStateService>().TriggerSceneTransition(BuildScenes.Ingame);
        }

        public void Show() {
            transform.position = _startPos + Vector3.down * Screen.height;
            iTween.MoveTo(gameObject, _startPos, 1);
            iTween.ScaleTo(gameObject, Vector3.one, 1);
            StartCoroutine(Coroutines.FadeTo(_canvasGroup, 1, .5f));
        }

        private void Hide() {
            iTween.MoveTo(gameObject, _startPos + Vector3.down * Screen.height, 1);
            iTween.ScaleTo(gameObject, Vector3.one * .5f, 1);
            StartCoroutine(Coroutines.FadeTo(_canvasGroup, 0, .5f));
        }
    }
}