using System;
using Rhythm.Services;
using Rhythm.Units;
using Rhythm.Utils;
using UnityEngine;

namespace Rhythm.Levels {
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class FinishLine : MonoBehaviour {
        private Action _update = Constants.Noop;
        private SpriteRenderer _renderer;
        private Collider2D _collider;
        private UnitService _unitService;
        private void Start() {
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<BoxCollider2D>();
            _update = CheckIfVisible;
            _unitService = ServiceLocator.Get<UnitService>();
        }

        private void Update() {
            _update();
        }

        private void CheckIfVisible() {
            if (_renderer.isVisible) {
                _update = CheckUnitProximity;
            }
        }

        private void CheckUnitProximity() {
            foreach (Unit unit in _unitService.GetAllUnits()) {
                if (unit.Owner == Constants.PLAYER_ID_PLAYER &&
                    unit.GetComponent<Collider2D>().Distance(_collider).isOverlapped) {
                    _update = Constants.Noop;
                    ServiceLocator.Get<GameStateService>().TriggerGameFinished();
                    return;
                }
            }
        }
    }
}