using System;
using System.Linq;
using Rhythm.Services;
using UnityEngine;

namespace Rhythm.Songs {
    public class Song {
        public event Action<BeatQuality, int> CommandExecuted;
        public event Action CommandExecutionFinished;
        public event Action CommandExecutionUpdate;
        private readonly float[] _beats;
        public string Name { get; }

        public Song(float[] beats, string name) {
            Name = name;
            if (beats.Any(beat => beat >= 4) || Mathf.Abs(beats[0] - 0) > float.Epsilon) {
                throw new Exception("A song mustn't be longer than 4 beats and must start at 0!");
            }

            _beats = beats;
        }

        public bool Contains(float[] beats) {
            if (beats.Length > _beats.Length) {
                return false;
            }
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (int i = 0; i < beats.Length; i++) {
                if (Mathf.Abs(beats[i] - _beats[i]) > BeatInputService.FAIL_TOLERANCE) {
                    return false;
                }
            }

            return true;
        }

        public bool Matches(float[] beats) {
            return beats.Length == _beats.Length && Contains(beats);
        }

        public void ExecuteCommand(BeatQuality beatQuality, int streakLength) {
            CommandExecuted?.Invoke(beatQuality, streakLength);
        }

        public void FinishCommandExecution() {
            CommandExecutionFinished?.Invoke();
        }

        public void ExecuteCommandUpdate() {
            CommandExecutionUpdate?.Invoke();
        }
    }
}