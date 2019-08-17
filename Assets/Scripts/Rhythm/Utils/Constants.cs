using System;
using UnityEngine.Events;

namespace Rhythm.Utils {
    public static class Constants {
        public static readonly UnityAction Noop = () => { };
        public static readonly UnityAction<float> NoopFloat = val => { };
        public const int REQUIRED_STREAK_SCORE = 16;
        public const int MAX_STREAK_POWER = 5;
        public const string LAYER_TOUCHABLES = "Touchables";
    }

    public enum BuildScenes {
        None = -1,
        Preload = 0,
        Intro = 1,
        Ingame = 2,
        Overworld = 3,
        IntroGather = 4
    }
}