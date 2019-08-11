using Rhythm.Items;
using Rhythm.Services;
using Rhythm.Units;
using UnityEngine;

namespace Rhythm.Commands {    
    public class GatherCommandProvider: CommandProvider {
        private ItemDeposit _closestDeposit;
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
        }

        private void DepositDepleted() {
            _unit.RemoveVisibleDeposit(_closestDeposit);
            UpdateClosestDeposit();
        }

        private void UpdateClosestDeposit() {
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
            if (direction.sqrMagnitude > .5f) {
                _unit.transform.Translate(Time.deltaTime * _unit.MovementSpeed * direction.normalized);
            } else {
                _closestDeposit.Collect(_unit, 1 * Time.deltaTime);
            }
            // move to target deposit and gather until empty, then move to next deposit if available and continue
        }
    }
}