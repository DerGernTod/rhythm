using Rhythm.Services;
using Rhythm.Units;
using UnityEngine;

namespace Rhythm.Commands {    
    public class MarchCommandProvider: CommandProvider {
        public override void ExecutionFinished(Unit unit) {
        }

        public override void Executed(BeatQuality beatQuality, int streak, Unit unit) {
        }

        public override void CommandUpdate(Unit unit) {
            unit.transform.Translate(Time.deltaTime * unit.MovementSpeed * Vector3.up);
        }
    }
}