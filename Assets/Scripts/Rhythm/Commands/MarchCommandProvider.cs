using Rhythm.Services;
using Rhythm.Units;
using UnityEngine;

namespace Rhythm.Commands {    
    public class MarchCommandProvider: CommandProvider {
        public override void ExecutionFinished() {
        }

        public override void Executed(NoteQuality noteQuality, int streak) {
        }

        public override void CommandUpdate() {
            _unit.transform.Translate(Time.deltaTime * _unit.MovementSpeed * Vector3.up);
        }
    }
}