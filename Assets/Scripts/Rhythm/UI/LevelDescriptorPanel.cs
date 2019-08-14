using System;
using Rhythm.Data;
using Rhythm.Levels;
using Rhythm.Persistence;
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
        [SerializeField] private RectTransform resourcesDetailsPanel;
        [SerializeField] private RectTransform enemiesDetailsPanel;
        [SerializeField] private Text heading;
        [SerializeField] private Image detailsPrefab;
        [SerializeField] private Sprite unknownResourceSprite;
#pragma warning restore 0649
        private LevelData _currentLevel;
        private CanvasGroup _canvasGroup;
        private Vector3 _startPos;
        private PlayerStore _currentPlayer;

        private void Start() {
            cancelButton.onClick.AddListener(Hide);
            playButton.onClick.AddListener(StartLevel);
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
            Hide();
            _startPos = transform.position;
            _currentPlayer = ServiceLocator.Get<PersistenceService>().CurrentPlayer;
        }

        public void SetLevel(LevelData level) {
            _currentLevel = level;
            heading.text = "Start Level " + level.name;
            resourcesDetailsPanel.transform.parent.gameObject.SetActive(level.depositProbability.Length > 0);
            enemiesDetailsPanel.transform.parent.gameObject.SetActive(false);
            int counter = 0;
            int NUM_DETAILS_ROWS = 3;
            int NUM_DETAILS_COLUMNS = 3;
            Rect resourcesDetailsPanelRect = resourcesDetailsPanel.rect;
            float columnWidth = resourcesDetailsPanelRect.width / NUM_DETAILS_COLUMNS;
            float rowHeight = resourcesDetailsPanelRect.height / NUM_DETAILS_ROWS;
            foreach (DepositProbability probability in level.depositProbability) {
                int column = counter % NUM_DETAILS_COLUMNS;
                int row = counter / NUM_DETAILS_COLUMNS;
                ItemData itemData = probability.deposit;
                Sprite sprite = unknownResourceSprite;
                if (_currentPlayer.HasDiscoveredItem(itemData)) {
                    sprite = itemData.sprite;
                }

                Image image = Instantiate(detailsPrefab, resourcesDetailsPanel);
                image.sprite = sprite;
                image.GetComponent<RectTransform>().anchoredPosition = new Vector2(column * columnWidth, row * rowHeight);

                counter++;
            }
        }

        private void StartLevel() {
            GameStateService gameStateService = ServiceLocator.Get<GameStateService>();
            gameStateService.CurrentLevelData = _currentLevel;
            gameStateService.TriggerSceneTransition(BuildScenes.Ingame);
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