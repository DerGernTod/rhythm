using Services;
using Units;
using UnityEngine;

namespace State {
    public class IngameState : MonoBehaviour {
        public void Start() {
            Unit firstUnit = ServiceLocator.Get<UnitService>().Create("Circle");
        }
    }
}