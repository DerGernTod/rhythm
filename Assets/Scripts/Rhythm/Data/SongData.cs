using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rhythm.Data {
    [Serializable]
    [CreateAssetMenu(fileName = "Data", menuName = "Rhythm/SongData", order = 1)]
    public class SongData : ScriptableObject {
        [FormerlySerializedAs("Beats")] public float[] beats;
        public AudioClip[] streak0Clips;
        public AudioClip[] streak1Clips;
        public AudioClip[] streak2Clips;
        public AudioClip[] streak3Clips;
        public AudioClip[] streak4Clips;
    }
}