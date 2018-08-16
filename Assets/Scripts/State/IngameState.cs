using Levels;
using Managers;
using Services;
using Units;
using UnityEngine;

namespace State {
    public class IngameState : MonoBehaviour {
        [SerializeField] private LevelData _levelData;
        private void Start() {
            LoopingBackground background = new GameObject().AddComponent<LoopingBackground>();
            background.transform.Translate(Vector3.forward * 1);
            background.Initialize(_levelData);
            Unit firstUnit = ServiceLocator.Get<UnitService>().Create("Circle");
            firstUnit.transform.Translate(Vector3.up * -8);
        }
    }
}