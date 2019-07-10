using System.Collections.Generic;
using System.Linq;
using Rhythm.Services;
using Units;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rhythm.Camera {
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class FollowAllUnits : MonoBehaviour {
        private UnitService _unitService;
        private Dictionary<int, Unit>.ValueCollection _units;
        private Vector3 _curTarget;
        [FormerlySerializedAs("_offset")] [SerializeField] private Vector3 offset = Vector3.zero;
        private void Awake() {
            _unitService = ServiceLocator.Get<UnitService>();
            _unitService.UnitCreated += UnitServiceOnUnitCreated;
            _unitService.UnitDestroyed += UnitServiceOnUnitDestroyed;
            _units = _unitService.GetAllUnits();
            _curTarget = CalcAveragePosition();
        }

        private void UnitServiceOnUnitDestroyed(Unit obj) {
            _units = _unitService.GetAllUnits();
        }

        private void UnitServiceOnUnitCreated(Unit obj) {
            _units = _unitService.GetAllUnits();
        }

        private void OnDestroy() {
            _unitService.UnitCreated -= UnitServiceOnUnitCreated;
            _unitService.UnitDestroyed -= UnitServiceOnUnitDestroyed;
        }

        private Vector3 CalcAveragePosition() {
            return _units.Aggregate(new Vector3(0, 0, 0), (vector3, unit) => vector3 + unit.transform.position) / _units.Count + offset;
        }

        private void Update() {
            _curTarget = CalcAveragePosition();
            transform.position = Vector3.Lerp(transform.position, _curTarget, Time.deltaTime);
        }
    }
}