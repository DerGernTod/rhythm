using System.Collections;
using Rhythm.Levels;
using Rhythm.Managers;
using Rhythm.Services;
using Rhythm.Units;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Rhythm.State {
    public class IngameState : MonoBehaviour {
        
#pragma warning disable 0649
        [SerializeField] private LevelData levelData;
        [SerializeField] private Text countdownText;
#pragma warning restore 0649
        private void Start() {
            LoopingBackground background = new GameObject("Looping Background").AddComponent<LoopingBackground>();
            background.transform.Translate(Vector3.forward * 1);
            background.Initialize(levelData);
            Level level = new GameObject("Level").AddComponent<Level>();
            level.Initialize(levelData);
            Unit firstUnit = ServiceLocator.Get<UnitService>().CreateUnit("Circle");
            Unit drummer = ServiceLocator.Get<UnitService>().CreateUnit("Drummer");
            firstUnit.transform.Translate(Vector3.up * -8);
            drummer.transform.Translate(Vector3.up * -8.25f);
            ServiceLocator.Get<GameStateService>().GameFinishing += OnGameFinishing;
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
            ServiceLocator.Get<GameStateService>().TriggerGameStarted();
            yield return new WaitForSeconds(BeatInputService.NOTE_TIME);
            StartCoroutine(Coroutines.FadeTo(countdownText.GetComponent<CanvasGroup>(), 0, BeatInputService.NOTE_TIME));

        }

        private void OnGameFinishing() {
            Debug.Log("Game finished!");
        }

        private void OnDestroy() {
            ServiceLocator.Get<GameStateService>().GameFinishing -= OnGameFinishing;
        }
    }
}