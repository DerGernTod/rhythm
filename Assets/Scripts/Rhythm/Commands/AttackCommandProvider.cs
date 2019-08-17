using System.Collections;
using Rhythm.Items;
using Rhythm.Services;
using Rhythm.Units;
using Rhythm.Utils;
using UnityEngine;

namespace Rhythm.Commands {
    public class AttackCommandProvider: CommandProvider {
        private Unit _closestEnemy;
        private int _curStreakPower;
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
                StartCoroutine(MoveToInitPos());
            }
        }

        private IEnumerator MoveToInitPos() {
            float curTime = 0;
            while (curTime < BeatInputService.NOTE_TIME * 2f) {
                curTime += Time.deltaTime;
                Vector3 dir = _initPosition - unit.transform.position;
                unit.transform.Translate(Time.deltaTime * unit.MovementSpeed * dir.normalized);
                yield return null;
            }
        }

        public override void Executed(NoteQuality noteQuality, int streak) {
            _closestEnemy = unit.GetClosestEnemy();
            _curStreakPower = streak;
            _initPosition = unit.transform.position;
            unit.Agent.speed = unit.MovementSpeed + unit.MovementSpeed * CalcStreakBonus();
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
            float streakMod = CalcStreakBonus();
            Vector3 direction = (_closestEnemy.transform.position - unit.transform.position).normalized;
            unit.Agent.destination = _closestEnemy.transform.position - direction * unit.Range;
            if (unit.Agent.path.status != UnityEngine.AI.NavMeshPathStatus.PathComplete) {
                UnityEngine.AI.NavMeshHit hit;
                if (unit.Agent.Raycast(unit.Agent.destination, out hit)) {
                    unit.Agent.destination = hit.position;
                }
            }
            if (unit.IsInRange(_closestEnemy.transform)) {
                _closestEnemy.TakeDamage(unit, streakMod);
            }
        }

        private float CalcStreakBonus() {
            return _curStreakPower * 1f / Constants.MAX_STREAK_POWER;
        }
    }
}