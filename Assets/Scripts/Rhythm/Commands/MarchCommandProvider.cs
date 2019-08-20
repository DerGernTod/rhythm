using System.Collections;
using Rhythm.Services;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace Rhythm.Commands {
    public class MarchCommandProvider: CommandProvider {

        public override void ExecutionFinished() {
            Invoke(nameof(StopAgent), BeatInputService.NOTE_TIME * 2);
        }

        public override void Executed(NoteQuality noteQuality, int streak) {
            unit.Agent.isStopped = false;
            unit.Agent.speed = unit.MovementSpeed + unit.MovementSpeed * streakBonus;
            unit.Agent.destination = unit.transform.position + Vector3.up * 20;
            NavMeshHit hit;
            // true if terminated before reaching max distance
            if (unit.Agent.SamplePathPosition(NavMesh.AllAreas, unit.Agent.remainingDistance, out hit)) {
                unit.Agent.destination = hit.position;
            }
        }

        public override void CommandUpdate() {
        }
    }
}