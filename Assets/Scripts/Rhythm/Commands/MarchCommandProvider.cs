using Rhythm.Services;
using Rhythm.Units;
using Rhythm.Utils;
using UnityEngine;

namespace Rhythm.Commands {    
    public class MarchCommandProvider: CommandProvider {
        private int streakPower;
        public override void ExecutionFinished() {
        }

        public override void Executed(NoteQuality noteQuality, int streak) {
            streakPower = streak;
        }

        public override void CommandUpdate() {
            float unitMovementSpeed = _unit.MovementSpeed + _unit.MovementSpeed * streakPower / Constants.MAX_STREAK_POWER;
            _unit.transform.Translate(Time.deltaTime * unitMovementSpeed * Vector3.up);
        }
    }
}