using Rhythm.Tiers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rhythm.Tools {
    [CreateAssetMenu(fileName = "Data", menuName = "Rhythm/Tools/ToolData", order = 3)]
    public class ToolData: ScriptableObject {
        [FormerlySerializedAs("toolTier")] public TierData tier;
        public ToolTypeData type;
    }
}