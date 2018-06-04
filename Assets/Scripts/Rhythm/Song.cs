using System;
using System.Linq;
using UnityEngine;

namespace Rhythm {
    public abstract class Song {
        private readonly float[] _beats;

        protected Song(float[] beats) {
            if (beats.Any(beat => beat >= 4)) {
                throw new Exception("A song mustn't be longer than 4 beats!");
            }

            _beats = beats;
        }

        public bool Contains(float[] beats) {
            for (int i = beats.Length - 1; i >= 0; i--) {
                if (Mathf.Abs(beats[i] - _beats[i]) > .25f) {
                    return false;
                }
            }

            return true;
        }

        public bool Matches(float[] beats) {
            return beats.Length == _beats.Length && Contains(beats);
        }

        public abstract void ExecuteCommand();
    }
}