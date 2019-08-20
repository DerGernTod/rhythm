using System;
using Rhythm.Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rhythm.Data {
	
	[Serializable]
	public struct DepositProbability {
		public ItemData deposit;
		[FormerlySerializedAs("min")] public float minAppearence;
        [FormerlySerializedAs("max")] public float maxAppearence;
		[FormerlySerializedAs("minHealth")] public float minContent;
		[FormerlySerializedAs("maxHealth")] public float maxContent;
	}
	
	[CreateAssetMenu(fileName = "Data", menuName = "Rhythm/LevelData", order = 5)]
	public class LevelData : ScriptableObject {
		[FormerlySerializedAs("BackgroundSprite")] public Sprite backgroundSprite;
		public DepositProbability[] depositProbability;
		public int length;
	}
}