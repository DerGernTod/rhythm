using Rhythm.Data;
using Rhythm.Tools;
using Rhythm.Units;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rhythm.Data {
    [CreateAssetMenu(fileName = "Data", menuName = "Rhythm/UnitData", order = 3)]
    public class UnitData : ScriptableObject {
        [FormerlySerializedAs("Health")] public int health;
        [FormerlySerializedAs("MovementSpeed")] public float movementSpeed;
        [FormerlySerializedAs("WeaponData")] public WeaponData weaponData;
        public ToolData toolData;
        // TODO: graphics
        [FormerlySerializedAs("Prefab")] public Unit prefab;
        [FormerlySerializedAs("CommandData")] public CommandData[] commandData;
    }
}