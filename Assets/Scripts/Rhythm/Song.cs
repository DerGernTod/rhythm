using System;
using System.Globalization;
using System.Linq;
using Managers;
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
            if (beats.Length > _beats.Length) {
                return false;
            }
            Debug.Log("Comparing received beats " 
                      + string.Join(", ", beats.Select(b => b.ToString(CultureInfo.CurrentCulture)))
                      + " to song: " 
                      + string.Join(", ", _beats.Select(b => b.ToString(CultureInfo.CurrentCulture))));
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

        public abstract void ExecuteCommand();
    }
}