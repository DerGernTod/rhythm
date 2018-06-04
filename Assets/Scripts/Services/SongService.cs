using System.Collections.Generic;
using System.Linq;
using Rhythm;
using Rhythm.Songs;
using UnityEngine;

namespace Services {
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

        public Song CheckSongs(float[] beats) {
            List<Song> matchingSongs = _songs.Where(song => song.Contains(beats)).ToList();
            if (matchingSongs.Count != 1) {
                Debug.Log("More than one song matches!");
            } else if (matchingSongs[0].Matches(beats)){
                return matchingSongs[0];
            }

            return null;
        }
    }
}