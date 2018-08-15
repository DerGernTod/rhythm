using Services;
using UnityEngine;

namespace Units {
	public class Unit : MonoBehaviour {
		private static int _ids;
		private int _unitId;
		private string _name;
		private int _health;
		private float _movementSpeed;
		private Sprite _sprite;

		public void Initialize(UnitData unitData) {
			_name = unitData.name;
			_health = unitData.Health;
			_movementSpeed = unitData.MovementSpeed;
			_sprite = unitData.Sprite;
			_unitId = _ids++;
			SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
			spriteRenderer.sprite = _sprite;
			// TODO: handle unitData.WeaponData
			ServiceLocator.Get<SongService>().Get("March").OnCommandExecuted += OnMarch;
		}

		private void OnDestroy() {
			ServiceLocator.Get<SongService>().Get("March").OnCommandExecuted -= OnMarch;
		}

		private void OnMarch(BeatQuality quality) {
			Debug.Log("Unit " + _unitId + " " + _name + " marching!");
		}
	}
}
