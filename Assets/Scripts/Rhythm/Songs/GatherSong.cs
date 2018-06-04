using UnityEngine;

namespace Rhythm.Songs {
    public class GatherSong : Song {
        public GatherSong() : base(new float[] { 0, 1, 1.5f, 2, 3 }) {}

        public override void ExecuteCommand() {
            Debug.Log("Gathering");
        }
    }
}