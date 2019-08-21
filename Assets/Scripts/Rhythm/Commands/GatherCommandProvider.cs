using System.Collections;
using Rhythm.Items;
using Rhythm.Services;
using Rhythm.Units;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace Rhythm.Commands {
    public class GatherCommandProvider: CommandProvider {
        private const float GATHER_DISTANCE = .6f;
        private ItemDeposit _closestDeposit;
        private Vector3 _initPosition;
        private UnitService _unitService;

        private void Awake() {
            _unitService = ServiceLocator.Get<UnitService>();
            _unitService.UnitDying += OnUnitDying;
            _unitService.UnitDisappeared += OnUnitDying;
        }

        private void OnUnitDying(Unit unit) {
            if (_closestDeposit && unit == _closestDeposit.Unit) {
                UpdateClosestDeposit();
            }
        }

        public override void ExecutionFinished() {
            if (!_closestDeposit) {
                unit.Agent.destination = _initPosition;
                unit.Agent.speed = unit.MovementSpeed;
            }
            Invoke(nameof(StopAgent), BeatInputService.NOTE_TIME * 2f);
        }

        public override void Executed(NoteQuality noteQuality, int streak) {
            // do nothing if current target still has health
            // otherwise search deposits on screen (closest first) and pick a target
            // pick targets depending on unit equipment (tier, type)
            _closestDeposit = unit.GetClosestDeposit();

            _initPosition = unit.transform.position;
            unit.Agent.isStopped = false;
            unit.Agent.speed = unit.MovementSpeed + unit.MovementSpeed * streakBonus;
            if (_closestDeposit) {
                Vector3 direction = (_closestDeposit.transform.position - unit.transform.position).normalized;
                unit.Agent.destination = _closestDeposit.transform.position - direction * GATHER_DISTANCE;
            }

        }

        private void UpdateClosestDeposit() {
            _closestDeposit = unit.GetClosestDeposit();
        }
        protected new void OnDestroy() {
            base.OnDestroy();
            _unitService.UnitDying -= OnUnitDying;
            _unitService.UnitDisappeared -= OnUnitDying;
        }

        public override void CommandUpdate() {
            if (!_closestDeposit) {
                return;
            }
            float obstacleRadius = _closestDeposit.Obstacle.radius;
            Vector3 direction = (_closestDeposit.transform.position - unit.transform.position).normalized;
            Vector3 destination = _closestDeposit.transform.position - direction * obstacleRadius;
        
            if ((unit.transform.position - destination).magnitude <= GATHER_DISTANCE) {
                float dps = 1 * Time.deltaTime;
                _closestDeposit.Collect(unit, dps + dps * streakBonus);
                unit.Agent.isStopped = true;
            } else {
                unit.Agent.isStopped = false;
                unit.Agent.destination = destination;
                NavMeshHit hit;
                // if the maxrange target isn't reachable, set enemy as target
                if (unit.Agent.SamplePathPosition(NavMesh.AllAreas, unit.Agent.remainingDistance, out hit)) {
                    unit.Agent.destination = _closestDeposit.transform.position;
                }
                // if enemy target isn't reachable, set max reachable to target
                if (unit.Agent.SamplePathPosition(NavMesh.AllAreas, unit.Agent.remainingDistance, out hit)) {
                    unit.Agent.destination = hit.position;
                }
                Debug.DrawLine(unit.transform.position, unit.Agent.destination);
            }
        }
    }
}