using System;
using System.Collections.Generic;
using Rhythm.Data;

namespace Rhythm.Persistence {
    [Serializable]
    public class PlayerStore {
        [field:NonSerialized]
        public event Action<string> OnSongLearned;
        public List<string> KnownSongs { get; }
        public string Name { get; }

        public PlayerStore(string name) {
            Name = name;
            KnownSongs = new List<string>();
        }

        public void LearnSong(string songName) {
            if (!KnownSongs.Contains(songName)) {
                KnownSongs.Add(songName);
                OnSongLearned?.Invoke(songName);
            }
        }
    }
}