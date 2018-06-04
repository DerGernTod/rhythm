using UnityEngine;

namespace Rhythm.Songs {
    public class MarchSong : Song{
        public MarchSong() : base(new float[] {0, 1, 2, 3}) {}

        public override void ExecuteCommand() {
            Debug.Log("Marching");
        }
    }
}