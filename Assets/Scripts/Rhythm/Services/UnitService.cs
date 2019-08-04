using System;
using System.Collections.Generic;
using System.Linq;
using Rhythm.Data;
using Rhythm.Units;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Rhythm.Services {
	public class UnitService : IService {
		private readonly UnitDataDictionary _unitsData = new UnitDataDictionary();
		private readonly UnitDictionary _createdUnits = new UnitDictionary();
		public event Action<Unit> UnitCreated;
		public event Action<Unit> UnitDestroyed;

		public void Initialize() {
			UnitData[] units = Resources.LoadAll<UnitData>("data/units");
			foreach (UnitData unit in units) {
				_unitsData.Add(unit.name, unit);
			}
			_createdUnits.Clear();
		}

		public void PostInitialize() {
			ServiceLocator.Get<GameStateService>().SceneTransitionStarted += (from, to) => OnSceneTransitionStarted();
			SceneManager.activeSceneChanged += (from, to) => OnSceneTransitionStarted();
		}

		private void OnSceneTransitionStarted() {
			foreach (KeyValuePair<int,Unit> createdUnit in _createdUnits) {
				UnitDestroyed?.Invoke(createdUnit.Value);
			}
			_createdUnits.Clear();
		}

		public void Destroy() {
			_unitsData.Clear();
			_createdUnits.Clear();
		}

		public Unit CreateUnit(string name) {
			UnitData unitData;
			if (!_unitsData.TryGetValue(name, out unitData)) {
				throw new Exception("Requested unit of type '" + name + "' not available." +
				                    "Create a corresponding UnitData first.");
			}

			Unit unit = Object.Instantiate(unitData.prefab);
			_createdUnits.Add(unit.GetInstanceID(), unit);
			UnitCreated?.Invoke(unit);
			unit.Initialize(unitData);
			return unit;
		}

		public void DestroyUnit(int id) {
			Unit unit;
			if (!_createdUnits.TryGetValue(id, out unit)) {
				Debug.LogWarning("Tried to destroy unit with id " + id + " but it wasn't available.");
				return;
			}
			_createdUnits.Remove(id);
			UnitDestroyed?.Invoke(unit);
		}

		public List<Unit> GetAllUnits() {
			return _createdUnits.Values.ToList();
		}
	}
}
