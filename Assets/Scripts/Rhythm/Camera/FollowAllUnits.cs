using System;
using System.Collections.Generic;
using System.Linq;
using Rhythm.Services;
using Rhythm.Units;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Rhythm.Camera {
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class FollowAllUnits : MonoBehaviour {
        private UnitService _unitService;
        private GameStateService _gameStateService;
        private List<Unit> _units;
        private Vector3 _curTarget;
        private UnityAction _update = Constants.Noop;
        [FormerlySerializedAs("_offset")] [SerializeField] private Vector3 offset = Vector3.zero;
        private void Awake() {
            _unitService = ServiceLocator.Get<UnitService>();
            _unitService.UnitCreated += UnitServiceOnUnitCreated;
            _unitService.UnitDestroyed += UnitServiceOnUnitDestroyed;
            _units = _unitService.GetAllPlayerUnits().ToList();
            _gameStateService = ServiceLocator.Get<GameStateService>();
            _gameStateService.GameFinishing += OnGameFinishing;
            if (_units.Count != 0) {
                _curTarget.y = CalcAveragePosition();
                _update = UpdateTargetToUnitAvg;
            } else {
                _curTarget = transform.position;
            }
        }

        private void OnGameFinishing() {
            _update = Constants.Noop;
            _curTarget += Vector3.up * 2f;
        }

        private void UnitServiceOnUnitDestroyed(Unit obj) {
            if (obj.Owner != OwnerType.PLAYER) {
                return;
            }
            _units.Remove(obj);
            if (_units.Count == 0) {
                _update = Constants.Noop;
            }
        }

        private void UnitServiceOnUnitCreated(Unit obj) {
            if (obj.Owner != OwnerType.PLAYER) {
                return;
            }
            _units.Add(obj);
            if (_units.Count != 0) {
                _update = UpdateTargetToUnitAvg;
            }
        }

        private void OnDestroy() {
            _unitService.UnitCreated -= UnitServiceOnUnitCreated;
            _unitService.UnitDestroyed -= UnitServiceOnUnitDestroyed;
            _gameStateService.GameFinishing -= OnGameFinishing;
            _update = Constants.Noop;
            _units = null;
        }

        private float CalcAveragePosition() {
            return _units.Average(unit => unit.transform.position.y + offset.y);
        }

        private void Update() {
            _update();
            transform.position = Vector3.Lerp(transform.position, _curTarget, Time.deltaTime);
        }

        private void UpdateTargetToUnitAvg() {

            _curTarget.y = CalcAveragePosition();
        }
    }
}