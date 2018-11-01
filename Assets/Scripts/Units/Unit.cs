using System;
using Rhythm;
using Rhythm.Commands;
using Services;
using UnityEngine;
using Utils;

namespace Units {
	public class Unit : MonoBehaviour {
		private static int _ids;
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

		public void Initialize(UnitData unitData) {
			_name = unitData.name;
			_health = unitData.Health;
			MovementSpeed = unitData.MovementSpeed;
			_prefab = unitData.Prefab;
			_unitId = _ids++;
			_commandData = unitData.CommandData;
			GameObject visuals = Instantiate(_prefab);
			visuals.transform.parent = transform;
			visuals.transform.localPosition = Vector3.zero;
			
			// TODO: handle unitData.WeaponData
			_executions = new Action<BeatQuality, int>[_commandData.Length];
			_finishes = new Action[_commandData.Length];
			_updates = new Action[_commandData.Length];
			for (int i = 0; i < _commandData.Length; i++) {
				CommandData commandData = _commandData[i];
				string songName = commandData.Song;
				_executions[i] = (quality, streakLenght) =>
					commandData.SongCommandExecuted(quality, streakLenght, this);
				_finishes[i] = () => commandData.SongCommandExecutionFinished(this);
				_updates[i] = () => commandData.SongCommandUpdate(this);
				ServiceLocator.Get<SongService>().Get(songName).CommandExecuted += _executions[i];
				ServiceLocator.Get<SongService>().Get(songName).CommandExecutionFinished += _finishes[i];
				ServiceLocator.Get<SongService>().Get(songName).CommandExecutionUpdate += _updates[i];
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
				string songName = commandData.Song;
				ServiceLocator.Get<SongService>().Get(songName).CommandExecuted -= _executions[i];
				ServiceLocator.Get<SongService>().Get(songName).CommandExecutionFinished -= _finishes[i];
				ServiceLocator.Get<SongService>().Get(songName).CommandExecutionUpdate -= _updates[i];
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
