using System;
using System.Collections.Generic;
using System.Linq;
using Rhythm.Data;
using Rhythm.Units;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Rhythm.Services {
	public class UnitService : IUpdateableService {
		private readonly UnitDataDictionary _unitsData = new UnitDataDictionary();
		private readonly UnitDictionary _createdUnits = new UnitDictionary();
        private readonly LinkedList<Unit> _visibleUnits = new LinkedList<Unit>();
        private readonly LinkedList<Unit> _invisibleUnits = new LinkedList<Unit>();
		public event UnityAction<Unit> UnitCreated;
		public event UnityAction<Unit> UnitDestroyed;
        public event UnityAction<Unit> UnitDying;
        public event UnityAction<Unit> UnitDied;
        public event UnityAction<Unit> UnitAppeared;
        public event UnityAction<Unit> UnitDisappeared;

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
            Destroy();
		}

		public void Destroy() {
            _invisibleUnits.Clear();
            _visibleUnits.Clear();
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
            _visibleUnits.AddLast(unit);
			unit.Initialize(unitData);
			UnitCreated?.Invoke(unit);
			return unit;
		}

        public void AddUnit(Unit unit) {
            _createdUnits.Add(unit.GetInstanceID(), unit);
            _visibleUnits.AddLast(unit);
            UnitCreated?.Invoke(unit);
        }

        public void RemoveUnit(Unit unit) {
            DestroyUnit(unit.GetInstanceID());
        }

        public void DestroyUnit(int id) {
            Debug.Log("Destroying unit with id " + id);
			Unit unit;
			if (!_createdUnits.TryGetValue(id, out unit)) {
				Debug.LogWarning("Tried to destroy unit with id " + id + " but it wasn't available.");
				return;
			}
			_createdUnits.Remove(id);
            if (_visibleUnits.Contains(unit)) {
                UnitDisappeared?.Invoke(unit);
            }
            _visibleUnits.Remove(unit);
            _invisibleUnits.Remove(unit);
			UnitDestroyed?.Invoke(unit);
		}

		public List<Unit> GetAllUnits() {
			return _createdUnits.Values.ToList();
        }

        public IEnumerable<Unit> GetAllPlayerUnits() {
            return _createdUnits.Values.Where(unit => unit.Owner == OwnerType.PLAYER);
        }

        public IEnumerable<Unit> GetAllEnemyUnits() {
            return _createdUnits.Values.Where(unit => unit.Owner != OwnerType.PLAYER);
        }

        public void Update(float deltaTime) {
            HandleVisibility();

        }

        private void HandleVisibility() {
            LinkedListNode<Unit> curNode = _visibleUnits.First;
            int addedCount = 0;
            while (curNode != null) {
                Unit unit = curNode.Value;
                LinkedListNode<Unit> next = curNode.Next;
                if (!unit.IsVisible) {
                    UnitDisappeared?.Invoke(unit);
                    _visibleUnits.Remove(unit);
                    _invisibleUnits.AddFirst(unit);
                    addedCount++;
                }
                curNode = next;
            }
            curNode = _invisibleUnits.First;
            while (curNode != null && addedCount > 0) {
                curNode = curNode.Next;
                addedCount--;
            }
            while (curNode != null) {
                Unit unit = curNode.Value;
                LinkedListNode<Unit> next = curNode.Next;
                if (unit.IsVisible) {
                    UnitAppeared?.Invoke(unit);
                    _invisibleUnits.Remove(unit);
                    _visibleUnits.AddFirst(unit);
                }
                curNode = next;
            }
        }

        public void FixedUpdate() {
            
        }

        public void TriggerUnitDying(Unit unit) {
            UnitDying?.Invoke(unit);
        }

        public void TriggerUnitDied(Unit unit) {
            UnitDied?.Invoke(unit);
        }
    }
}
