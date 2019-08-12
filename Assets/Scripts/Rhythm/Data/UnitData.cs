using Rhythm.Tools;
using Rhythm.Units;
using Rhythm.Weapons;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rhythm.Data {
    [CreateAssetMenu(fileName = "Data", menuName = "Rhythm/UnitData", order = 3)]
    public class UnitData : ScriptableObject {
        [FormerlySerializedAs("Health")] public int health;
        [FormerlySerializedAs("MovementSpeed")] public float movementSpeed;
        [FormerlySerializedAs("WeaponData")] public WeaponData weaponData;
        // TODO: graphics
        [FormerlySerializedAs("Prefab")] public Unit prefab;
        [FormerlySerializedAs("CommandData")] public CommandData[] commandData;
        public ToolData toolData;
    }
}