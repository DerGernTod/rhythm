using Services;
using Units;
using UnityEngine;

namespace Rhythm.Commands {
    public abstract class CommandData: ScriptableObject {
        public string Song;
        public abstract void SongCommandExecutionFinished(Unit unit);
        public abstract void SongCommandExecuted(BeatQuality beatQuality, int streak, Unit unit);
        public abstract void SongCommandUpdate(Unit unit);
    }
}