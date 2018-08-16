using UnityEngine;

namespace Levels {
	[CreateAssetMenu(fileName = "Data", menuName = "Rhythm/LevelData", order = 1)]
	public class LevelData : ScriptableObject {
		public Sprite BackgroundSprite;
	}
}