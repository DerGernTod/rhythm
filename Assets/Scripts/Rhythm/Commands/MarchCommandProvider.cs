using System.Collections;
using Rhythm.Services;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace Rhythm.Commands {
    public class MarchCommandProvider: CommandProvider {
        private int streakPower;
        public override void ExecutionFinished() {
            // StartCoroutine(ContinueMovingForAWhile());
            unit.Agent.destination = unit.transform.position + Vector3.up * 2;
            unit.Agent.speed = 0;
        }

        private IEnumerator ContinueMovingForAWhile() {
            float curTime = 0;
            float maxTime = BeatInputService.NOTE_TIME * 2;
            float maxSpeed = unit.MovementSpeed + unit.MovementSpeed * streakPower / Constants.MAX_STREAK_POWER;
            while (curTime < maxTime) {
                float progress = curTime / maxTime;
                float curSpeed = Mathf.Lerp(maxSpeed, 0, progress);
                unit.transform.Translate(Time.deltaTime * curSpeed * Vector3.up);
                curTime += Time.deltaTime;
                yield return null;
            }
        }

        public override void Executed(NoteQuality noteQuality, int streak) {
            streakPower = streak;
            unit.Agent.speed = unit.MovementSpeed + CalcMovementSpeedBonus();
            Vector3 dest = unit.transform.position + Vector3.up * 20;
            unit.Agent.destination = dest;
            if(unit.Agent.path.status != NavMeshPathStatus.PathComplete) {
                NavMeshHit hit;
                if (unit.Agent.Raycast(unit.Agent.destination, out hit)) {
                    unit.Agent.destination = hit.position;
                }
            }
        }

        public override void CommandUpdate() {
            
        }

        private float CalcMovementSpeedBonus() {
            return unit.MovementSpeed * streakPower / Constants.MAX_STREAK_POWER;
        }
    }
}