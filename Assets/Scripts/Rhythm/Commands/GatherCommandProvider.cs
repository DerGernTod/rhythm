using Rhythm.Items;
using Rhythm.Services;
using Rhythm.Units;
using Rhythm.Utils;
using UnityEngine;

namespace Rhythm.Commands {    
    public class GatherCommandProvider: CommandProvider {
        private ItemDeposit _closestDeposit;
        private int curStreakPower = 0;
        public override void ExecutionFinished() {
            // if target still has health, keep it
        }

        public override void Executed(NoteQuality noteQuality, int streak) {
            // do nothing if current target still has health
            // otherwise search deposits on screen (closest first) and pick a target
            // pick targets depending on unit equipment (tier, type)
            if (_closestDeposit) {
                _closestDeposit.DepositDepleted -= DepositDepleted;
            }
            _closestDeposit = _unit.GetClosestDeposit();
            if (_closestDeposit) {
                _closestDeposit.DepositDepleted += DepositDepleted;
            }

            curStreakPower = streak;
        }

        private void DepositDepleted() {
            _unit.RemoveVisibleDeposit(_closestDeposit);
            UpdateClosestDeposit();
        }

        private void UpdateClosestDeposit() {
            Debug.Log(Time.time + ": " + _unit.name + " searching for new deposit");
            _closestDeposit = _unit.GetClosestDeposit();
            if (_closestDeposit) {
                _closestDeposit.DepositDepleted += DepositDepleted;
            }
        }

        public override void CommandUpdate() {
            if (!_closestDeposit) {
                return;
            }

            Vector3 direction = _closestDeposit.transform.position - _unit.transform.position;
            if (direction.sqrMagnitude > .35f) {
                float speed = _unit.MovementSpeed + _unit.MovementSpeed * curStreakPower / Constants.MAX_STREAK_POWER;
                _unit.transform.Translate(Time.deltaTime * speed * direction.normalized);
            } else {
                float dps = 1 * Time.deltaTime;
                _closestDeposit.Collect(_unit, dps + dps * curStreakPower / Constants.MAX_STREAK_POWER);
            }
            // move to target deposit and gather until empty, then move to next deposit if available and continue
        }
    }
}