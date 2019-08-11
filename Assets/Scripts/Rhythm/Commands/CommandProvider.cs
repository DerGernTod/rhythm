using Rhythm.Services;
using Rhythm.Units;
using UnityEngine;

namespace Rhythm.Commands {
    public abstract class CommandProvider: MonoBehaviour {
        protected Unit _unit;

        public void RegisterUnit(Unit unit) {
            _unit = unit;
        }
        public abstract void ExecutionFinished();
        public abstract void Executed(NoteQuality noteQuality, int streak);
        public abstract void CommandUpdate();
    }
}