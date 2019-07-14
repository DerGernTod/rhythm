using Rhythm.Levels;
using Rhythm.Managers;
using Rhythm.Services;
using Rhythm.Units;
using UnityEngine;

namespace State {
    public class IngameState : MonoBehaviour {
        
#pragma warning disable 0649
        [SerializeField] private LevelData levelData;
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
            ServiceLocator.Get<GameStateService>().GameFinished += OnGameFinished;
        }

        private void OnGameFinished() {
            Debug.Log("Game finished!");
        }

        private void OnDestroy() {
            ServiceLocator.Get<GameStateService>().GameFinished -= OnGameFinished;
        }
    }
}