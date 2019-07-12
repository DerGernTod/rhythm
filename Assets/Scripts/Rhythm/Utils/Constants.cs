using System;

namespace Rhythm.Utils {
    public static class Constants {
        public static readonly Action Noop = () => { };
        public static readonly Action<float> NoopFloat = (float val) => { };
        public const int PLAYER_ID_PLAYER = 0;
        public const int PLAYER_ID_AI_0 = 1;
    }
}