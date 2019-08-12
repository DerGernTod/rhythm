using System;
using System.Collections;
using Rhythm.Data;
using Rhythm.Levels;
using Rhythm.Managers;
using Rhythm.Services;
using Rhythm.Units;
using Rhythm.Utils;
using TheNode.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Rhythm.State {
    public class IngameState : MonoBehaviour {
        
#pragma warning disable 0649
        [SerializeField] private Text countdownText;
        [SerializeField] private AnimatedText finishText;
        [SerializeField] private RectTransform collectedItemsPanel;
        [SerializeField] private RectTransform collectedItemPrefab;
        [SerializeField] private Text timeText;
        [SerializeField] private CanvasGroup summaryCanvas;
        private GameStateService _gameStateService;
#pragma warning restore 0649
        private float startTime;
        private Level _level;

        private void Start() {
            _gameStateService = ServiceLocator.Get<GameStateService>();
            LevelData levelData = _gameStateService.CurrentLevelData;
            LoopingBackground background = new GameObject("Looping Background").AddComponent<LoopingBackground>();
            background.transform.Translate(Vector3.forward * 1);
            background.Initialize(levelData);
            _level = new GameObject("Level").AddComponent<Level>();
            _level.Initialize(levelData);
            Unit firstUnit = ServiceLocator.Get<UnitService>().CreateUnit("Circle");
            Unit drummer = ServiceLocator.Get<UnitService>().CreateUnit("Drummer");
            firstUnit.transform.Translate(Vector3.up * -8);
            drummer.transform.Translate(Vector3.up * -8.25f);
            _gameStateService.GameFinishing += OnGameFinishing;
            StartCoroutine(StartGame());
        }

        private IEnumerator StartGame() {
            int remainingTicks = 3;
            while (remainingTicks > 0) {
                remainingTicks--;
                countdownText.text = "" + (remainingTicks + 1);
                iTween.PunchScale(countdownText.gameObject, Vector3.one * 2f, BeatInputService.HALF_NOTE_TIME);
                yield return new WaitForSeconds(BeatInputService.NOTE_TIME);
            }

            countdownText.text = "Drum!";
            iTween.PunchScale(countdownText.gameObject, Vector3.one * 2f, BeatInputService.HALF_NOTE_TIME);
            yield return new WaitForSeconds(BeatInputService.HALF_NOTE_TIME);
            _gameStateService.TriggerGameStarted();
            startTime = Time.time;
            yield return new WaitForSeconds(BeatInputService.NOTE_TIME);
            StartCoroutine(Coroutines.FadeTo(countdownText.GetComponent<CanvasGroup>(), 0, BeatInputService.NOTE_TIME));

        }

        private void OnGameFinishing() {
            float finishTime = Time.time - startTime;
            TimeSpan fromMilliseconds = TimeSpan.FromSeconds(finishTime);
            timeText.text = fromMilliseconds.Minutes + ":" + fromMilliseconds.Seconds;
            StartCoroutine(Coroutines.FadeTo(finishText.GetComponent<CanvasGroup>(), 1, BeatInputService.NOTE_TIME));
            StartCoroutine(Coroutines.FadeTo(summaryCanvas.GetComponent<CanvasGroup>(), 1, BeatInputService.NOTE_TIME));
            int count = 0;
            Rect summaryPanelRect = collectedItemsPanel.rect;
            float columnWidth = summaryPanelRect.width / 3f;
            float rowHeight = summaryPanelRect.height / 3f;
            foreach (ItemData levelCollectedItem in _level.CollectedItems) {
                int row = count / 3;
                int column = count % 3;
                RectTransform collectedItem = Instantiate(collectedItemPrefab, collectedItemsPanel);
                collectedItem.anchoredPosition = new Vector2(column * columnWidth, (2 - row) * rowHeight);
                collectedItem.GetComponentInChildren<Image>().sprite = levelCollectedItem.sprite;
                collectedItem.GetComponentInChildren<Text>().text = "1";
                count++;
            }
            iTween.MoveFrom(summaryCanvas.gameObject, new Vector3(Screen.width * .5f, Screen.height * .5f, 0), 1);
            iTween.MoveFrom(finishText.gameObject, new Vector3(Screen.width * .5f, Screen.height * .5f, 0), 1);
            finishText.StartAnimation();
            finishText.TriggerImpulse();
        }

        private void OnDestroy() {
            _gameStateService.GameFinishing -= OnGameFinishing;
        }
    }
}