using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private const int NUM_SUMMARY_ROWS = 3;
        private const int NUM_SUMMARY_COLUMNS = 3;
        
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
            Unit secondUnit = ServiceLocator.Get<UnitService>().CreateUnit("Circle");
            Unit drummer = ServiceLocator.Get<UnitService>().CreateUnit("Drummer");
            firstUnit.transform.Translate(Vector3.up * -8);
            secondUnit.transform.Translate(Vector3.up * -8 + Vector3.right);
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
            StartCoroutine(AnimateSummary());
            StartCoroutine(Coroutines.FadeTo(finishText.GetComponent<CanvasGroup>(), 1, BeatInputService.NOTE_TIME));

            iTween.MoveFrom(finishText.gameObject, new Vector3(Screen.width * .5f, Screen.height * .5f, 0), 1);
            finishText.StartAnimation();
            finishText.TriggerImpulse();
        }

        private IEnumerator AnimateSummary() {
            yield return new WaitForSeconds(.5f);
            iTween.MoveFrom(summaryCanvas.gameObject, new Vector3(Screen.width * .5f, Screen.height * .5f, 0), 1);
            StartCoroutine(Coroutines.FadeTo(summaryCanvas.GetComponent<CanvasGroup>(), 1, BeatInputService.NOTE_TIME));
            yield return new WaitForSeconds(1f);
            Rect summaryPanelRect = collectedItemsPanel.rect;
            float columnWidth = summaryPanelRect.width / NUM_SUMMARY_COLUMNS;
            float rowHeight = summaryPanelRect.height / NUM_SUMMARY_ROWS;
            Dictionary<ItemData, int> dict =
                _level.CollectedItems.Aggregate(new Dictionary<ItemData, int>(), (curDict, item) => {
                    int amount;
                    curDict.TryGetValue(item, out amount);
                    amount++;
                    curDict[item] = amount;
                    return curDict;
                });
            int count = 0;
            foreach (KeyValuePair<ItemData, int> keyValuePair in dict) {
                int row = count / NUM_SUMMARY_COLUMNS;
                int column = count % NUM_SUMMARY_COLUMNS;
                RectTransform collectedItem = Instantiate(collectedItemPrefab, collectedItemsPanel);
                collectedItem.anchoredPosition = new Vector2(column * columnWidth, (NUM_SUMMARY_ROWS - 1 - row) * rowHeight);
                collectedItem.GetComponentInChildren<Image>().sprite = keyValuePair.Key.sprite;
                collectedItem.GetComponentInChildren<Text>().text = keyValuePair.Value + "";
                CanvasGroup canvasGroup = collectedItem.GetComponent<CanvasGroup>();
                canvasGroup.alpha = 0;
                StartCoroutine(Coroutines.FadeTo(canvasGroup, 1, .25f));
                iTween.ScaleFrom(collectedItem.gameObject, Vector3.one * 1.5f, .5f);
                yield return new WaitForSeconds(.5f);
                count++;
            }
        }

        private void OnDestroy() {
            _gameStateService.GameFinishing -= OnGameFinishing;
        }
    }
}