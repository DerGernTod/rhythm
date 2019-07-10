using UnityEngine;

namespace Rhythm.Tiers {
    [CreateAssetMenu(fileName = "Data", menuName = "Rhythm/TierData", order = 7)]
    public class TierData: ScriptableObject {
        public int quality;
        public Sprite sprite;
        public string qualityName;
    }
}