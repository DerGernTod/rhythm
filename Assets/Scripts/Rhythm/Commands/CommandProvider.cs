using Rhythm.Services;
using Rhythm.Units;
using UnityEngine;

namespace Rhythm.Commands {
    public abstract class CommandProvider: MonoBehaviour {
        public abstract void ExecutionFinished(Unit unit);
        public abstract void Executed(NoteQuality noteQuality, int streak, Unit unit);
        public abstract void CommandUpdate(Unit unit);
    }
}