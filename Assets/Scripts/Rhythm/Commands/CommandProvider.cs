using Rhythm.Services;
using Rhythm.Songs;
using Rhythm.Units;
using Rhythm.Utils;
using UnityEngine;

namespace Rhythm.Commands {
    public abstract class CommandProvider: MonoBehaviour {
        protected Unit unit;
        protected float streakBonus;
        private int _curStreakPower;
        private Song _song;

        public void RegisterUnit(Unit regUnit) {
            unit = regUnit;
        }

        public void RegisterSong(Song targetSong) {
            _song = targetSong;
            _song.CommandExecuted += OnCommandExecuted;
            _song.CommandExecutionFinished += ExecutionFinished;
            _song.CommandExecutionUpdate += CommandUpdate;
        }

        protected void OnDestroy() {
            _song.CommandExecuted -= OnCommandExecuted;
            _song.CommandExecutionFinished -= ExecutionFinished;
            _song.CommandExecutionUpdate -= CommandUpdate;
        }

        private float CalcStreakBonus() {
            return _curStreakPower * 1f / Constants.MAX_STREAK_POWER;
        }
        private void OnCommandExecuted(NoteQuality noteQuality, int streak) {
            _curStreakPower = streak;
            streakBonus = CalcStreakBonus();
            Executed(noteQuality, streak);
        }

        public abstract void ExecutionFinished();
        public abstract void Executed(NoteQuality noteQuality, int streak);
        public abstract void CommandUpdate();

        protected void StopAgent() {
            unit.Agent.isStopped = true;
        }
    }
}