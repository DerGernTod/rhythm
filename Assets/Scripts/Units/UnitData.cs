using Rhythm;
using Rhythm.Commands;
using UnityEngine;
using Weapons;

namespace Units {
    [CreateAssetMenu(fileName = "Data", menuName = "Rhythm/UnitData", order = 3)]
    public class UnitData : ScriptableObject {
        public int Health;
        public float MovementSpeed;
        public WeaponData WeaponData;
        // TODO: graphics
        public GameObject Prefab;
        public CommandData[] CommandData;
    }
}