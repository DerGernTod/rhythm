using System;
using System.Collections;
using System.Collections.Generic;
using Rhythm.Commands;
using Rhythm.Data;
using Rhythm.Items;
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
		private List<ItemDeposit> _depositsInSight;
		private CommandData[] _commandData;
		private CommandProvider[] _commandProviders;
		private BeatInputService _beatInputService;
		private GameStateService _gameStateService;

		private void Awake() {
			_updateFunc = Constants.Noop;
			_depositsInSight = new List<ItemDeposit>();
		}

		public void Initialize(UnitData unitData) {
			_name = unitData.name;
			_health = unitData.health;
			MovementSpeed = unitData.movementSpeed;
			Owner = Constants.PLAYER_ID_PLAYER;
			_unitId = ids++;
			_commandData = unitData.commandData;
			_commandProviders = new CommandProvider[_commandData.Length];
			// TODO: handle unitData.WeaponData
			for (int i = 0; i < _commandData.Length; i++) {
				CommandData commandData = _commandData[i];
				CommandProvider commandProvider = Instantiate(commandData.commandProviderPrefab, transform);
				commandProvider.RegisterUnit(this);
				string songName = commandData.song.name;
				Song song = ServiceLocator.Get<SongService>().Get(songName);
				song.CommandExecuted += commandProvider.Executed;
				song.CommandExecutionFinished += commandProvider.ExecutionFinished;
				song.CommandExecutionUpdate += commandProvider.CommandUpdate;
				_commandProviders[i] = commandProvider;
			}

			_beatInputService = ServiceLocator.Get<BeatInputService>();
			_beatInputService.BeatLost += BeatLost;
			_beatInputService.ExecutionFinishing += ExecutionFinishing;
			_gameStateService = ServiceLocator.Get<GameStateService>();
			_gameStateService.GameFinishing += OnGameFinishing;
		}

		public ItemDeposit GetClosestDeposit() {
			if (_depositsInSight.Count == 0) {
				return null;
			}
			Vector3 transformPosition = transform.position;
			_depositsInSight.Sort((a, b) => (int)(Vector3.SqrMagnitude(a.transform.position - transformPosition)
			                                      - Vector3.SqrMagnitude(b.transform.position - transformPosition)));
			return _depositsInSight[0];
		}

		public void AddVisibleDeposit(ItemDeposit deposit) {
			for (int i = 0; i < _depositsInSight.Count; i++) {
				if (Vector3.SqrMagnitude(transform.position - _depositsInSight[i].transform.position)
				    > Vector3.SqrMagnitude(transform.position - deposit.transform.position)) {
					_depositsInSight.Insert(i, deposit);
					return;
				}
			}
			_depositsInSight.Add(deposit);
		}

		public void RemoveVisibleDeposit(ItemDeposit deposit) {
			_depositsInSight.Remove(deposit);
		}

		private void OnGameFinishing() {
			StartCoroutine(WalkThroughFinishLine());
		}

		private IEnumerator WalkThroughFinishLine() {
			Renderer curRenderer = GetComponent<Renderer>();
			while (curRenderer.isVisible) {
				transform.Translate(MovementSpeed * Time.deltaTime * Vector2.up);
				yield return null;
			}
		}

		private void Update() {
			_updateFunc();
		}

		private void OnDestroy() {
			for (int i = 0; i < _commandProviders.Length; i++) {
				CommandData commandData = _commandData[i];
				CommandProvider commandProvider = _commandProviders[i];
				string songName = commandData.song.name;
				Song song = ServiceLocator.Get<SongService>().Get(songName);
				song.CommandExecuted -= commandProvider.Executed;
				song.CommandExecutionFinished -= commandProvider.ExecutionFinished;
				song.CommandExecutionUpdate -= commandProvider.CommandUpdate;
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
			// do drop anims and sounds
			_updateFunc = Constants.Noop;
		}
	}
}
