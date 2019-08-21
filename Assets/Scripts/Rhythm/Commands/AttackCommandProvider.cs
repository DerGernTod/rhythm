using System.Collections;
using Rhythm.Items;
using Rhythm.Services;
using Rhythm.Units;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace Rhythm.Commands {
    public class AttackCommandProvider: CommandProvider {
        private Unit _closestEnemy;
        private Vector3 _initPosition;
        private UnitService _unitService;

        private void Awake() {
            _unitService = ServiceLocator.Get<UnitService>();
            _unitService.UnitDying += OnUnitDying;
            _unitService.UnitDisappeared += OnUnitDying;
        }

        protected new void OnDestroy() {
            base.OnDestroy();
            _unitService.UnitDying -= OnUnitDying;
            _unitService.UnitDisappeared -= OnUnitDying;
        }

        private void OnUnitDying(Unit unit) {
            if (unit == _closestEnemy) {
                UpdateClosestVisibleEnemy();
            }
        }

        public override void ExecutionFinished() {
            if (!_closestEnemy) {
                unit.Agent.destination = _initPosition;
                unit.Agent.speed = unit.MovementSpeed;
            }
            Invoke(nameof(StopAgent), BeatInputService.NOTE_TIME * 2f);
        }

        public override void Executed(NoteQuality noteQuality, int streak) {
            UpdateClosestVisibleEnemy();
            _initPosition = unit.transform.position;
            unit.Agent.isStopped = false;
            unit.Agent.speed = unit.MovementSpeed + unit.MovementSpeed * streakBonus;
            if (_closestEnemy) {
                Vector3 direction = (_closestEnemy.transform.position - unit.transform.position).normalized;
                unit.Agent.destination = _closestEnemy.transform.position - direction * unit.Range;
            }
        }

        private void UpdateClosestVisibleEnemy() {
            _closestEnemy = unit.GetClosestEnemy();
        }

        public override void CommandUpdate() {
            if (!_closestEnemy) {
                return;
            }
            if (unit.IsInRange(_closestEnemy.transform)) {
                float damage = (unit.Damage + unit.Damage * streakBonus) * Time.deltaTime;
                _closestEnemy.TakeDamage(unit, damage);
                unit.Agent.isStopped = true;
            } else {
                unit.Agent.isStopped = false;
                Vector3 direction = (_closestEnemy.transform.position - unit.transform.position).normalized;
                unit.Agent.destination = _closestEnemy.transform.position - direction * unit.Range * .9f;
                NavMeshHit hit;
                // if the maxrange target isn't reachable, set enemy as target
                if (unit.Agent.SamplePathPosition(NavMesh.AllAreas, unit.Agent.remainingDistance, out hit)) {
                    Debug.Log("Maxrange target not reachable from " + unit.name + unit.gameObject.GetInstanceID() + ", taking enemy position");
                    unit.Agent.destination = _closestEnemy.transform.position;
                }
                // if enemy target isn't reachable, set max reachable to target
                if (unit.Agent.SamplePathPosition(NavMesh.AllAreas, unit.Agent.remainingDistance, out hit)) {
                    Debug.Log("Maxrange target not reachable from " + unit.name + unit.gameObject.GetInstanceID() + ", taking hit position");
                    unit.Agent.destination = hit.position;
                }
                Debug.DrawLine(unit.transform.position, unit.Agent.destination);
            }
        }
    }
}