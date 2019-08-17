using Rhythm.Services;
using Rhythm.Songs;
using Rhythm.Units;
using UnityEngine;

namespace Rhythm.Commands {
    public abstract class CommandProvider: MonoBehaviour {
        protected Unit unit;
        private Song song;

        public void RegisterUnit(Unit regUnit) {
            unit = regUnit;
        }

        public void RegisterSong(Song targetSong) {
            song = targetSong;
            song.CommandExecuted += Executed;
            song.CommandExecutionFinished += ExecutionFinished;
            song.CommandExecutionUpdate += CommandUpdate;
        }

        protected void OnDestroy() {
            song.CommandExecuted -= Executed;
            song.CommandExecutionFinished -= ExecutionFinished;
            song.CommandExecutionUpdate -= CommandUpdate;
        }
        public abstract void ExecutionFinished();
        public abstract void Executed(NoteQuality noteQuality, int streak);
        public abstract void CommandUpdate();
    }
}