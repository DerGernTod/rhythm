﻿using Rhythm.Weapons;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rhythm.Data {
    [CreateAssetMenu(fileName = "Data", menuName = "Rhythm/WeaponData", order = 4)]
    public class WeaponData : ScriptableObject {
        [FormerlySerializedAs("AttackType")] public AttackType attackType;
        [FormerlySerializedAs("TargetType")] public TargetType targetType;
        [FormerlySerializedAs("TargetShape")] public Vector2[] targetShape;
        [FormerlySerializedAs("Damage")] public int damage;
        public DamageType tamageType;
    }
}