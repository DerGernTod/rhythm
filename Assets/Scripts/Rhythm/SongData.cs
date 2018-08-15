using UnityEngine;

namespace Rhythm {
    [CreateAssetMenu(fileName = "Data", menuName = "Rhythm/SongData", order = 1)]
    public class SongData : ScriptableObject {
        public float[] Beats;
    }
}