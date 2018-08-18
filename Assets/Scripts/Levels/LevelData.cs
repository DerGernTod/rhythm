using UnityEngine;

namespace Levels {
	[CreateAssetMenu(fileName = "Data", menuName = "Rhythm/LevelData", order = 5)]
	public class LevelData : ScriptableObject {
		public Sprite BackgroundSprite;
	}
}