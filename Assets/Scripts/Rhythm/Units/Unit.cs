using System;
using System.Collections;
using Rhythm.Commands;
using Rhythm.Data;
using Rhythm.Services;
using Rhythm.Songs;
using Rhythm.Utils;
using UnityEngine;

namespace Rhythm.Units {
	[RequireComponent(typeof(Collider2D))]
	public class Unit : MonoBehaviour {
		private static int ids;
		
		private int _unitId;
		private string _name;
		private int _health;

		public int Owner { get; private set; }
		public float MovementSpeed { get; private set;}

		private Action _updateFunc;
		private CommandData[] _commandData;

		private Action<NoteQuality, int>[] _executions;
		private Action[] _finishes;
		private Action[] _updates;
		private BeatInputService _beatInputService;
		private GameStateService _gameStateService;

		private void Awake() {
			_updateFunc = Constants.Noop;
		}

		public void Initialize(UnitData unitData) {
			_name = unitData.name;
			_health = unitData.health;
			MovementSpeed = unitData.movementSpeed;
			Owner = Constants.PLAYER_ID_PLAYER;
			_unitId = ids++;
			_commandData = unitData.commandData;
			
			// TODO: handle unitData.WeaponData
			_executions = new Action<NoteQuality, int>[_commandData.Length];
			_finishes = new Action[_commandData.Length];
			_updates = new Action[_commandData.Length];
			for (int i = 0; i < _commandData.Length; i++) {
				CommandData commandData = _commandData[i];
				CommandProvider commandProvider = Instantiate(commandData.commandProviderPrefab, transform); 
				string songName = commandData.song.name;
				_executions[i] = (quality, streakLength) =>
					commandProvider.Executed(quality, streakLength, this);
				_finishes[i] = () => commandProvider.ExecutionFinished(this);
				_updates[i] = () => commandProvider.CommandUpdate(this);
				Song song = ServiceLocator.Get<SongService>().Get(songName);
				song.CommandExecuted += _executions[i];
				song.CommandExecutionFinished += _finishes[i];
				song.CommandExecutionUpdate += _updates[i];
			}

			_beatInputService = ServiceLocator.Get<BeatInputService>();
			_beatInputService.BeatLost += BeatLost;
			_beatInputService.ExecutionFinishing += ExecutionFinishing;
			_gameStateService = ServiceLocator.Get<GameStateService>();
			_gameStateService.GameFinishing += OnGameFinishing;
		}

		private void OnGameFinishing() {
			StartCoroutine(WalkThroughFinishLine());
		}

		private IEnumerator WalkThroughFinishLine() {
			while (true) {
				transform.Translate(MovementSpeed * Time.deltaTime * Vector2.up);
				yield return null;
			}
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
			_beatInputService.BeatLost -= BeatLost;
			_beatInputService.ExecutionFinishing -= ExecutionFinishing;
			_gameStateService.GameFinishing -= OnGameFinishing;
			StopAllCoroutines();
		}

		private void BeatLost() {
			_updateFunc = DropUpdate;
		}

		private void ExecutionFinishing(Song song) {
			_updateFunc = Constants.Noop;
		}
		
		private void DropUpdate() {
			Debug.Log("Unit drops down!");
		}
	}
}
