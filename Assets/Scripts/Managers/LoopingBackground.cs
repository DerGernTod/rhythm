using System;
using Levels;
using UnityEngine;

namespace Managers {
	public class LoopingBackground : MonoBehaviour {
		private LevelData _levelData;
		private SpriteRenderer[] _backgrounds;
		private bool[] _backgroundWasVisible;
		private const float BackgroundDistance = 12.8f;
		private Action _updateFunc;
		
		private void Awake() {
			_backgrounds = new SpriteRenderer[3];
			_backgroundWasVisible = new bool[3];
			_updateFunc = () => { };
		}
		
		public void Initialize(LevelData levelData) {
			_levelData = levelData;
			for (int i = 0; i < 3; i++) {
				SpriteRenderer spriteRenderer = new GameObject().AddComponent<SpriteRenderer>();
				spriteRenderer.sprite = _levelData.BackgroundSprite;
				spriteRenderer.transform.parent = transform;
				spriteRenderer.transform.localPosition = Vector3.down * BackgroundDistance * (i - 1);
				spriteRenderer.name = "Background" + i;
				_backgrounds[i] = spriteRenderer;
				_backgroundWasVisible[i] = spriteRenderer.isVisible;
			}

			_updateFunc = BackgroundUpdate;
		}

		private void BackgroundUpdate() {
			for (int i = 0; i < 3; i++) {
				if (_backgroundWasVisible[i] && !_backgrounds[i].isVisible) {
					_backgrounds[i].transform.Translate(0, BackgroundDistance * 3, 0);
				}

				_backgroundWasVisible[i] = _backgrounds[i].isVisible;
			}
		}
		
		private void Update() {
			_updateFunc();
		}
	}
}
