﻿using System;
using System.Collections.Generic;
using System.Linq;
using Rhythm.Songs;
using Rhythm.Utils;
using UnityEngine;

namespace Rhythm.Services {
    [Serializable]
    public class SongService : IService {
        
        private SongDictionary _songs;
        public void Initialize() {
            SongData[] songData = Resources.LoadAll<SongData>("songs");
            _songs = new SongDictionary();
            foreach (SongData song in songData) {
                Debug.Log("Loaded song " + song.name);
                _songs.Add(song.name, new Song(song.beats, song.name));
            }
        }

        public void PostInitialize() {
        }

        public void Destroy() {
            _songs.Clear();
        }

        public Song Get(string songName) {
            Song song;
            if (_songs.TryGetValue(songName, out song)) {
                return song;
            }
            throw new Exception("Unknown song with name " + songName + "!");
        }

        public List<Song> CheckSongs(float[] beats) {
            return _songs.Values.Where(song => song.Contains(beats)).ToList();
        }
    }
}