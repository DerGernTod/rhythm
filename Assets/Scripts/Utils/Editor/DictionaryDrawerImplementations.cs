using System;
using Rhythm;
using Services;
using Units;
using UnityEditor;
using UnityEngine;

namespace Utils.Editor {

    [CustomPropertyDrawer(typeof(QualityClipDictionary))]
    public class QualityClipDictionaryDrawer : DictionaryDrawer<QualityClipDictionary, BeatQuality, AudioClip> {}
    [CustomPropertyDrawer(typeof(ServiceDictionary))]
    public class ServiceDictionaryDrawer : DictionaryDrawer<ServiceDictionary, Type, IService> {}
    [CustomPropertyDrawer(typeof(UnitDictionary))]
    public class UnitDictionaryDrawer : DictionaryDrawer<UnitDictionary, string, UnitData> {}
    [CustomPropertyDrawer(typeof(SongDictionary))]
    public class SongDictionaryDrawer : DictionaryDrawer<SongDictionary, string, Song> {}
}