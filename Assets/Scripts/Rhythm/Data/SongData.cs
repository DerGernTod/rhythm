using UnityEngine;
using UnityEngine.Serialization;

namespace Rhythm.Songs {
    [CreateAssetMenu(fileName = "Data", menuName = "Rhythm/SongData", order = 1)]
    public class SongData : ScriptableObject {
        [FormerlySerializedAs("Beats")] public float[] beats;
    }
}