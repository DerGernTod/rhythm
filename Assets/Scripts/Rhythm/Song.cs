using System;
using System.Globalization;
using System.Linq;
using Services;
using UnityEngine;

namespace Rhythm {
    public class Song {
        public event Action<BeatQuality, int> OnCommandExecuted;
        public event Action OnCommandExecutionFinished;
        private readonly float[] _beats;

        public Song(float[] beats) {
            if (beats.Any(beat => beat >= 4) || Mathf.Abs(beats[0] - 0) > float.Epsilon) {
                throw new Exception("A song mustn't be longer than 4 beats and must start at 0!");
            }

            _beats = beats;
        }

        public bool Contains(float[] beats) {
            if (beats.Length > _beats.Length) {
                return false;
            }
            for (int i = 0; i < beats.Length; i++) {
                if (Mathf.Abs(beats[i] - _beats[i]) > BeatInputService.FailTolerance) {
                    return false;
                }
            }

            return true;
        }

        public bool Matches(float[] beats) {
            return beats.Length == _beats.Length && Contains(beats);
        }

        public void ExecuteCommand(BeatQuality beatQuality, int streakLength) {
            OnCommandExecuted?.Invoke(beatQuality, streakLength);
        }

        public void FinishCommandExecution() {
            OnCommandExecutionFinished?.Invoke();
        }
    }
}