﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhythm.Data;
using Rhythm.Units;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.AI;
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

        private MonoBehaviour _coroutineProvider;

        public UnitService(MonoBehaviour coroutineProvider) {
            _coroutineProvider = coroutineProvider;
        }

		public void Initialize() {
			UnitData[] units = Resources.LoadAll<UnitData>("data/units");
			foreach (UnitData unit in units) {
				_unitsData.Add(unit.name, unit);
			}
			_createdUnits.Clear();
            UnitAppeared += (unit) => Debug.Log("Appeared: " + unit.GetInstanceID() + ", " + unit.name);
            UnitDisappeared += (unit) => Debug.Log("Disppeared: " + unit.GetInstanceID() + ", " + unit.name);
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

		public Unit CreateUnit(string name, Vector3 position, OwnerType owner = OwnerType.NONE) {
			UnitData unitData;
			if (!_unitsData.TryGetValue(name, out unitData)) {
				throw new Exception("Requested unit of type '" + name + "' not available." +
				                    "Create a corresponding UnitData first.");
			}
            if (unitData.prefab.GetComponent<NavMeshAgent>()) {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(position, out hit, 1, NavMesh.AllAreas)) {
                    position = hit.position;
                } else {
                    Debug.LogWarning("Couldn't sample a position for new unit " + name + " on the navmesh!");
                }
            }

			Unit unit = Object.Instantiate(unitData.prefab, position, Quaternion.identity);
			_createdUnits.Add(unit.GetInstanceID(), unit);
            _visibleUnits.AddLast(unit);
			unit.Initialize(unitData, owner);
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
            return _createdUnits.Values.Where(unit => unit.Owner == OwnerType.AI);
        }
        public IEnumerable<Unit> GetAllNeutralUnits() {
            return _createdUnits.Values.Where(unit => unit.Owner == OwnerType.NEUTRAL);
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
