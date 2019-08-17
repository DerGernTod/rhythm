using System.Collections;
using Rhythm.Items;
using Rhythm.Services;
using Rhythm.Utils;
using UnityEngine;

namespace Rhythm.Commands {
    public class GatherCommandProvider: CommandProvider {
        private ItemDeposit _closestDeposit;
        private int curStreakPower;
        private Vector3 initPosition;
        public override void ExecutionFinished() {
            StartCoroutine(MoveToInitPos());
        }

        private IEnumerator MoveToInitPos() {
            float curTime = 0;
            while (curTime < BeatInputService.NOTE_TIME * 2f) {
                curTime += Time.deltaTime;
                Vector3 dir = initPosition - unit.transform.position;
                unit.transform.Translate(Time.deltaTime * unit.MovementSpeed * dir.normalized);
                yield return null;
            }
        }

        public override void Executed(NoteQuality noteQuality, int streak) {
            // do nothing if current target still has health
            // otherwise search deposits on screen (closest first) and pick a target
            // pick targets depending on unit equipment (tier, type)
            if (_closestDeposit) {
                _closestDeposit.DepositDepleted -= DepositDepleted;
            }
            _closestDeposit = unit.GetClosestDeposit();
            if (_closestDeposit) {
                _closestDeposit.DepositDepleted += DepositDepleted;
            }

            curStreakPower = streak;
            initPosition = unit.transform.position;
        }

        private void DepositDepleted() {
            unit.RemoveVisibleDeposit(_closestDeposit);
            UpdateClosestDeposit();
        }

        private void UpdateClosestDeposit() {
            _closestDeposit = unit.GetClosestDeposit();
            if (_closestDeposit) {
                _closestDeposit.DepositDepleted += DepositDepleted;
            }
        }

        public override void CommandUpdate() {
            if (!_closestDeposit) {
                return;
            }

            Vector3 direction = _closestDeposit.transform.position - unit.transform.position;
            if (direction.sqrMagnitude > .35f) {
                float speed = unit.MovementSpeed + unit.MovementSpeed * curStreakPower / Constants.MAX_STREAK_POWER;
                unit.transform.Translate(Time.deltaTime * speed * direction.normalized);
            } else {
                float dps = 1 * Time.deltaTime;
                _closestDeposit.Collect(unit, dps + dps * curStreakPower / Constants.MAX_STREAK_POWER);
            }
            // move to target deposit and gather until empty, then move to next deposit if available and continue
        }
    }
}