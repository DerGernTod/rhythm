using System.Collections;
using System.Collections.Generic;
using Rhythm.Services;
using Rhythm.Units;
using Rhythm.Utils;
using UnityEngine;

namespace Rhythm.Commands {    
    public class MarchCommandProvider: CommandProvider {
        private int streakPower;
        public override void ExecutionFinished() {
            StartCoroutine(ContinueMovingForAWhile());
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
        }

        public override void CommandUpdate() {
            float unitMovementSpeed = unit.MovementSpeed + unit.MovementSpeed * streakPower / Constants.MAX_STREAK_POWER;
            unit.transform.Translate(Time.deltaTime * unitMovementSpeed * Vector3.up);
        }
    }
}