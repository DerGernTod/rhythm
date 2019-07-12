using System;
using Rhythm.Levels;
using UnityEngine;

namespace Rhythm.Managers {
	public class LoopingBackground : MonoBehaviour {
		private LevelData _levelData;
		private SpriteRenderer[] _backgrounds;
		private bool[] _backgroundWasVisible;
		private const float BACKGROUND_DISTANCE = 12.8f;
		private Action _updateFunc;
		
		private void Awake() {
			_backgrounds = new SpriteRenderer[3];
			_backgroundWasVisible = new bool[3];
			_updateFunc = () => { };
		}
		
		public void Initialize(LevelData levelData) {
			_levelData = levelData;
			for (int i = 0; i < 3; i++) {
				SpriteRenderer spriteRenderer = new GameObject("Background" + i).AddComponent<SpriteRenderer>();
				spriteRenderer.sprite = _levelData.backgroundSprite;
				Transform transform1 = spriteRenderer.transform;
				transform1.parent = transform;
				transform1.localPosition = BACKGROUND_DISTANCE * (i - 1) * Vector3.down;
				_backgrounds[i] = spriteRenderer;
				_backgroundWasVisible[i] = spriteRenderer.isVisible;
			}

			_updateFunc = BackgroundUpdate;
		}

		private void BackgroundUpdate() {
			for (int i = 0; i < 3; i++) {
				if (_backgroundWasVisible[i] && !_backgrounds[i].isVisible) {
					_backgrounds[i].transform.Translate(0, BACKGROUND_DISTANCE * 3, 0);
				}

				_backgroundWasVisible[i] = _backgrounds[i].isVisible;
			}
		}
		
		private void Update() {
			_updateFunc();
		}
	}
}
