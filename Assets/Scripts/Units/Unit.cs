using System;
using Rhythm;
using Rhythm.Commands;
using Rhythm.Services;
using Rhythm.Songs;
using UnityEngine;
using Utils;

namespace Units {
	[RequireComponent(typeof(BoxCollider2D))]
	public class Unit : MonoBehaviour {
		private static int ids;
		private int _unitId;
		private string _name;
		private int _health;

		public float MovementSpeed { get; private set;}

		private GameObject _prefab;
		private Action _updateFunc;
		private CommandData[] _commandData;

		private Action<BeatQuality, int>[] _executions;
		private Action[] _finishes;
		private Action[] _updates;
		
		private void Awake() {
			_updateFunc = Constants.Noop;
		}

		private void Start() {
		}

		public void Initialize(UnitData unitData) {
			_name = unitData.name;
			_health = unitData.Health;
			MovementSpeed = unitData.MovementSpeed;
			_prefab = unitData.Prefab;
			_unitId = ids++;
			_commandData = unitData.CommandData;
			GameObject visuals = Instantiate(_prefab, transform);
			visuals.transform.localPosition = Vector3.zero;
			
			// TODO: handle unitData.WeaponData
			_executions = new Action<BeatQuality, int>[_commandData.Length];
			_finishes = new Action[_commandData.Length];
			_updates = new Action[_commandData.Length];
			for (int i = 0; i < _commandData.Length; i++) {
				CommandData commandData = _commandData[i];
				string songName = commandData.song.name;
				_executions[i] = (quality, streakLength) =>
					commandData.executed?.Invoke(quality, streakLength, this);
				_finishes[i] = () => commandData.executionFinished?.Invoke(this);
				_updates[i] = () => commandData.update?.Invoke(this);
				Song song = ServiceLocator.Get<SongService>().Get(songName);
				song.CommandExecuted += _executions[i];
				song.CommandExecutionFinished += _finishes[i];
				song.CommandExecutionUpdate += _updates[i];
			}
			
			ServiceLocator.Get<BeatInputService>().OnBeatLost += OnBeatLost;
			ServiceLocator.Get<BeatInputService>().OnExecutionFinished += OnExecutionFinished;
		}

		private void Update() {
			_updateFunc();
		}

		private void OnDestroy() {
			for (int i = 0; i < _commandData.Length; i++) {
				CommandData commandData = _commandData[i];
				string songName = commandData.song.name;
				Song song = ServiceLocator.Get<SongService>().Get(songName);
				song.CommandExecuted -= _executions[i];
				song.CommandExecutionFinished -= _finishes[i];
				song.CommandExecutionUpdate -= _updates[i];
			}
			ServiceLocator.Get<BeatInputService>().OnBeatLost -= OnBeatLost;
			ServiceLocator.Get<BeatInputService>().OnExecutionFinished -= OnExecutionFinished;
		}

		private void OnBeatLost() {
			_updateFunc = DropUpdate;
		}

		private void OnExecutionFinished(Song song) {
			_updateFunc = Constants.Noop;
		}
		
		private void DropUpdate() {
			
		}
	}
}
