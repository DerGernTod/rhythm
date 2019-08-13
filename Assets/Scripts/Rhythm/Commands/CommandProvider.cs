using Rhythm.Services;
using Rhythm.Units;
using UnityEngine;

namespace Rhythm.Commands {
    public abstract class CommandProvider: MonoBehaviour {
        protected Unit unit;

        public void RegisterUnit(Unit regUnit) {
            unit = regUnit;
        }
        public abstract void ExecutionFinished();
        public abstract void Executed(NoteQuality noteQuality, int streak);
        public abstract void CommandUpdate();
    }
}