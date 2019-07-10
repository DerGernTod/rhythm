using System;
using Rhythm.Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rhythm.Levels {
	
	[Serializable]
	public struct DepositProbability {
		public ItemData deposit;
		public float min;
		public float max;
	}
	
	[CreateAssetMenu(fileName = "Data", menuName = "Rhythm/LevelData", order = 5)]
	public class LevelData : ScriptableObject {
		[FormerlySerializedAs("BackgroundSprite")] public Sprite backgroundSprite;
		public DepositProbability[] depositProbability;
		public int length;
	}
}