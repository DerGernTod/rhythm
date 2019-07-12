using System;
using Rhythm.Data;
using Rhythm.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rhythm.Items {
    [Serializable]
    [CreateAssetMenu(fileName = "Data", menuName = "Rhythm/ItemData", order = 6)]
    public class ItemData: ScriptableObject {
        public Sprite sprite;
        public Sprite depositSprite;
        public string itemName;
        public int amount;
        [FormerlySerializedAs("requiredToolTier")] public TierData requiredTier;
        public ToolTypeData requiredToolType;
    }
}