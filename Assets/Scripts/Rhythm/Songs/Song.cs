using System;
using System.Linq;
using Rhythm.Services;
using UnityEngine;

namespace Rhythm.Songs {
    public class Song {
        public event Action<NoteQuality, int> CommandExecuted;
        public event Action CommandExecutionFinished;
        public event Action CommandExecutionUpdate;
        public string Name { get; }
        private readonly float[] _beats;
        private readonly AudioClip[][] _clips;

        public Song(float[] beats, string name, AudioClip[][] clips) {
            Name = name;
            if (beats.Any(beat => beat >= 4) || Mathf.Abs(beats[0] - 0) > float.Epsilon) {
                throw new Exception("A song mustn't be longer than 4 ticks and must start at 0!");
            }

            _clips = clips;
            _beats = beats;
        }

        public AudioClip[] GetClipsByStreakPower(int streakPower) {
            return _clips.Length > streakPower ? _clips[streakPower] : new AudioClip[0];
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

        public void ExecuteCommand(NoteQuality noteQuality, int streakLength) {
            CommandExecuted?.Invoke(noteQuality, streakLength);
        }

        public void FinishCommandExecution() {
            CommandExecutionFinished?.Invoke();
        }

        public void ExecuteCommandUpdate() {
            CommandExecutionUpdate?.Invoke();
        }
    }
}