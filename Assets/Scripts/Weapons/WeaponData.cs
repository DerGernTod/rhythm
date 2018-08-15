using UnityEngine;

namespace Weapons {
    [CreateAssetMenu(fileName = "Data", menuName = "Rhythm/WeaponData", order = 2)]
    public class WeaponData : ScriptableObject {
        public AttackType AttackType;
        public TargetType TargetType;
        public Vector2[] TargetShape;
        public int Damage;
    }
}