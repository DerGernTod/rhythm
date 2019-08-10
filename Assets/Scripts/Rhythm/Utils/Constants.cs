using System;

namespace Rhythm.Utils {
    public static class Constants {
        public static readonly Action Noop = () => { };
        public static readonly Action<float> NoopFloat = val => { };
        public const int REQUIRED_STREAK_SCORE = 16;
        public const int MAX_STREAK_POWER = 5;
        public const int PLAYER_ID_PLAYER = 0;
        public const int PLAYER_ID_AI_0 = 1;
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