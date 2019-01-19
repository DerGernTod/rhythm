﻿using Levels;
using Managers;
using Services;
using Units;
using UnityEngine;

#pragma warning disable 0649
namespace State {
    public class IngameState : MonoBehaviour {
        
        [SerializeField] private LevelData levelData;
        private void Start() {
            LoopingBackground background = new GameObject().AddComponent<LoopingBackground>();
            background.transform.Translate(Vector3.forward * 1);
            background.Initialize(levelData);
            Unit firstUnit = ServiceLocator.Get<UnitService>().CreateUnit("Circle");
            Unit drummer = ServiceLocator.Get<UnitService>().CreateUnit("Drummer");
            firstUnit.transform.Translate(Vector3.up * -8);
            drummer.transform.Translate(Vector3.up * -8.25f);
        }
    }
}