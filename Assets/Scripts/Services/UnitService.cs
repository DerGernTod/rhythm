using System;
using Units;
using UnityEngine;
using Utils;

namespace Services {
	public class UnitService : IService {
		private readonly UnitDictionary _units = new UnitDictionary();

		public void Initialize() {
			UnitData[] units = Resources.LoadAll<UnitData>("units");
			foreach (UnitData unit in units) {
				_units.Add(unit.name, unit);
			}
		}

		public void PostInitialize() {
		}

		public Unit Create(string name) {
			UnitData unitData;
			if (!_units.TryGetValue(name, out unitData)) {
				throw new Exception("Requested unit of type '" + name + "' not available." +
				                    "Create a corresponding UnitData first.");
			}

			GameObject g = new GameObject(name);
			Unit unit = g.AddComponent<Unit>();
			unit.Initialize(unitData);
			return unit;
		}
	}
}
