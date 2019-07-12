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
        [FormerlySerializedAs("Prefab")] public GameObject prefab;
        [FormerlySerializedAs("CommandData")] public CommandData[] commandData;
    }
}