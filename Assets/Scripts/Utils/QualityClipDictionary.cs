using System;
using Services;
using UnityEngine;

namespace Utils {
    [Serializable]
    public class QualityClipDictionary : SerializableDictionary<BeatQuality, AudioClip> {}

}