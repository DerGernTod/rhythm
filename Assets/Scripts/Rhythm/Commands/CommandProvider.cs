using Rhythm.Services;
using Rhythm.Units;
using UnityEngine;

namespace Rhythm.Commands {
    public abstract class CommandProvider: MonoBehaviour {
        public abstract void ExecutionFinished(Unit unit);
        public abstract void Executed(BeatQuality beatQuality, int streak, Unit unit);
        public abstract void CommandUpdate(Unit unit);
    }
}