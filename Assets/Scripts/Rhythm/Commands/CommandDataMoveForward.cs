using Services;
using Units;
using UnityEngine;
using Utils;

namespace Rhythm.Commands {
    [CreateAssetMenu(fileName = "MoveForwardContainer", menuName = "Rhythm/Commands/MoveFowrard", order = 1)]
    public class CommandDataMoveForward: CommandData {
        public override void SongCommandExecutionFinished(Unit unit) {
        }

        public override void SongCommandExecuted(BeatQuality beatQuality, int streak, Unit unit) {
        }

        public override void SongCommandUpdate(Unit unit) {
            unit.transform.Translate(Time.deltaTime * unit.MovementSpeed * Vector3.up);
        }
    }
}