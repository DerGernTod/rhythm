using System;
using UnityEngine;

namespace Rhythm.Data {
    [Serializable]
    [CreateAssetMenu(fileName = "Data", menuName = "Rhythm/TierData", order = 7)]
    public class TierData: ScriptableObject {
        public int quality;
        public Sprite sprite;
        public string qualityName;
    }
}