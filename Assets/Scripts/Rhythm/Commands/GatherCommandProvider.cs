using Rhythm.Services;
using Rhythm.Units;
using UnityEngine;

namespace Rhythm.Commands {    
    public class GatherCommandProvider: CommandProvider {
        public override void ExecutionFinished(Unit unit) {
            // if target still has health, keep it
        }

        public override void Executed(BeatQuality beatQuality, int streak, Unit unit) {
            // do nothing if current target still has health
            // otherwise search deposits on screen (closest first) and pick a target
            // pick targets depending on unit equipment (tier, type)
        }

        public override void CommandUpdate(Unit unit) {
            // move to target deposit and gather until empty, then move to next deposit if available and continue
            unit.transform.Translate(Time.deltaTime * unit.MovementSpeed * Vector3.up);
        }
    }
}