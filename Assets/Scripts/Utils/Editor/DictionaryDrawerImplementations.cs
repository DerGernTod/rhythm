using System;
using Rhythm;
using Rhythm.Items;
using Rhythm.Services;
using Rhythm.Songs;
using Units;
using UnityEditor;
using UnityEngine;

namespace Utils.Editor {

    [CustomPropertyDrawer(typeof(QualityClipDictionary))]
    public class QualityClipDictionaryDrawer : DictionaryDrawer<QualityClipDictionary, BeatQuality, AudioClip> {}
    [CustomPropertyDrawer(typeof(ServiceDictionary))]
    public class ServiceDictionaryDrawer : DictionaryDrawer<ServiceDictionary, Type, IService> {}
    [CustomPropertyDrawer(typeof(UnitDataDictionary))]
    public class UnitDictionaryDrawer : DictionaryDrawer<UnitDataDictionary, string, UnitData> {}
    [CustomPropertyDrawer(typeof(SongDictionary))]
    public class SongDictionaryDrawer : DictionaryDrawer<SongDictionary, string, Song> {}
}