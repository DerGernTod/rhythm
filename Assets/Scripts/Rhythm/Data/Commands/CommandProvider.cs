using Rhythm.Services;
using Units;
using UnityEngine;

namespace Rhythm.Commands {    
    [CreateAssetMenu(fileName = "CommandProvider", menuName = "Rhythm/Commands/CommandProvider", order = 1)]
    public class CommandProvider: ScriptableObject {
        public void MarchExecutionFinished(Unit unit) {
        }

        public void MarchExecuted(BeatQuality beatQuality, int streak, Unit unit) {
        }

        public void MarchUpdate(Unit unit) {
            unit.transform.Translate(Time.deltaTime * unit.MovementSpeed * Vector3.up);
        }

        public void GatherUpdate(Unit unit) {
            
        }
    }
}