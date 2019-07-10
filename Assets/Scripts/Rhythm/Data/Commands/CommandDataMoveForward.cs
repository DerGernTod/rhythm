using Rhythm.Services;
using Units;
using UnityEngine;
using Utils;

namespace Rhythm.Commands {
    public static class CommandDataMoveForward {
        public static void ExecutionFinished(Unit unit) {
        }

        public static void Executed(BeatQuality beatQuality, int streak, Unit unit) {
        }

        public static void Update(Unit unit) {
            unit.transform.Translate(Time.deltaTime * unit.MovementSpeed * Vector3.up);
        }
    }
}