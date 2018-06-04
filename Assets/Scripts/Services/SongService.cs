using System;
using System.Collections.Generic;
using System.Linq;
using Rhythm;
using Rhythm.Songs;

namespace Services {
    [Serializable]
    public class SongService : IService {
        private List<Song> _songs;
        public void Initialize() {
            _songs = new List<Song> {
                new GatherSong(),
                new MarchSong()
            };
        }

        public void PostInitialize() {
        }

        public List<Song> CheckSongs(float[] beats) {
            return _songs.Where(song => song.Contains(beats)).ToList();
        }
    }
}