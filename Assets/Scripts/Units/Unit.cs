using System;
using Rhythm;
using Services;
using UnityEngine;
using Utils;

namespace Units {
	public class Unit : MonoBehaviour {
		private static int _ids;
		private int _unitId;
		private string _name;
		private int _health;
		private float _movementSpeed;
		private Sprite _sprite;
		private Action _updateFunc;

		private void Awake() {
			_updateFunc = Constants.Noop;
		}

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
			ServiceLocator.Get<BeatInputService>().OnBeatLost += OnBeatLost;
			ServiceLocator.Get<BeatInputService>().OnExecutionFinished += OnExecutionFinished;
		}

		private void Update() {
			_updateFunc();
		}

		private void OnDestroy() {
			ServiceLocator.Get<SongService>().Get("March").OnCommandExecuted -= OnMarch;
			ServiceLocator.Get<BeatInputService>().OnBeatLost -= OnBeatLost;
			ServiceLocator.Get<BeatInputService>().OnExecutionFinished -= OnExecutionFinished;
		}

		private void OnBeatLost() {
			_updateFunc = DropUpdate;
		}

		private void OnMarch(BeatQuality quality, int streakLength) {
			Debug.Log("Unit " + _unitId + " " + _name + " marching!");
			_updateFunc = MarchUpdate;
		}

		private void OnExecutionFinished(Song song) {
			_updateFunc = Constants.Noop;
		}
		
		private void DropUpdate() {
			
		}


		private void MarchUpdate() {
			transform.Translate(Time.deltaTime * _movementSpeed * Vector3.up);
		}
	}
}
