using System;
using System.Collections.Generic;
using Rhythm.Data;

namespace Rhythm.Persistence {
    [Serializable]
    public class PlayerStore {
        public event Action<SongData> OnSongLearned;
        public List<SongData> KnownSongs { get; private set; }
        public string Name { get; private set; }

        public PlayerStore(string name) {
            Name = name;
            KnownSongs = new List<SongData>();
        }

        public void LearnSong(SongData song) {
            if (!KnownSongs.Contains(song)) {
                KnownSongs.Add(song);
                OnSongLearned?.Invoke(song);
            }
        }
    }
}