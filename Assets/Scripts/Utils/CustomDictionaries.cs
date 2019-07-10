﻿using System;
using Rhythm.Items;
using Rhythm.Services;
using Rhythm.Songs;
using Units;
using UnityEngine;

namespace Utils {
    [Serializable]
    public class ServiceDictionary: SerializableDictionary<Type, IService> {}

    [Serializable]
    public class QualityClipDictionary : SerializableDictionary<BeatQuality, AudioClip> {}

    [Serializable]
    public class UnitDataDictionary: SerializableDictionary<string, UnitData> {}

    [Serializable]
    public class UnitDictionary : SerializableDictionary<int, Unit> {}

    [Serializable]
    public class SongDictionary: SerializableDictionary<string, Song> {}
    
    [Serializable]
    public class DepositProbabilityDictionary : SerializableDictionary<ItemData, float> {}
}