using System;
using System.Collections.Generic;
using System.Linq;
using Rhythm.Services;
using Rhythm.Units;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rhythm.Camera {
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class FollowAllUnits : MonoBehaviour {
        private UnitService _unitService;
        private GameStateService _gameStateService;
        private List<Unit> _units;
        private Vector3 _curTarget;
        private Action _update;
        [FormerlySerializedAs("_offset")] [SerializeField] private Vector3 offset = Vector3.zero;
        private void Awake() {
            _unitService = ServiceLocator.Get<UnitService>();
            _unitService.UnitCreated += UnitServiceOnUnitCreated;
            _unitService.UnitDestroyed += UnitServiceOnUnitDestroyed;
            _units = _unitService.GetAllUnits();
            _gameStateService = ServiceLocator.Get<GameStateService>();
            _gameStateService.GameFinishing += OnGameFinishing;
            if (_units.Count != 0) {
                _update = UpdateTargetToUnitAvg;
            }
        }

        private void Start() {
            _curTarget = CalcAveragePosition();
        }

        private void OnGameFinishing() {
            _update = Constants.Noop;
            _curTarget += Vector3.up * 2f;
        }

        private void UnitServiceOnUnitDestroyed(Unit obj) {
            _units.Remove(obj);
            if (_units.Count == 0) {
                _update = Constants.Noop;
            }
        }

        private void UnitServiceOnUnitCreated(Unit obj) {
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

        private Vector3 CalcAveragePosition() {
            return _units.Aggregate(new Vector3(0, 0, 0), (vector3, unit) => vector3 + unit.transform.position) / _units.Count + offset;
        }

        private void Update() {
            _update();
            transform.position = Vector3.Lerp(transform.position, _curTarget, Time.deltaTime);
        }

        private void UpdateTargetToUnitAvg() {
            _curTarget = CalcAveragePosition();
        }
    }
}