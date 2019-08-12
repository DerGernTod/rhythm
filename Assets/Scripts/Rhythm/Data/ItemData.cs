using System;
using Rhythm.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rhythm.Data {
    [Serializable]
    [CreateAssetMenu(fileName = "Data", menuName = "Rhythm/ItemData", order = 6)]
    public class ItemData: ScriptableObject {
        public Sprite sprite;
        public Sprite depositSprite;
        public string itemName;
        [FormerlySerializedAs("requiredToolTier")] public TierData requiredTier;
        public ToolTypeData requiredToolType;
    }
}