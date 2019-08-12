using System;
using Rhythm.Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rhythm.Data {
	
	[Serializable]
	public struct DepositProbability {
		public ItemData deposit;
		public float min;
		public float max;
		public float minHealth;
		public float maxHealth;
	}
	
	[CreateAssetMenu(fileName = "Data", menuName = "Rhythm/LevelData", order = 5)]
	public class LevelData : ScriptableObject {
		[FormerlySerializedAs("BackgroundSprite")] public Sprite backgroundSprite;
		public DepositProbability[] depositProbability;
		public int length;
	}
}