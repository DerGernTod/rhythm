using System;
using System.Collections.Generic;
using System.Linq;
using Rhythm.Data;
using Rhythm.Persistence;
using Rhythm.Songs;
using Rhythm.Utils;
using UnityEngine;

namespace Rhythm.Services {
    [Serializable]
    public class SongService : IService {
        
        private SongDictionary _songs;
        private SongDictionary _knownSongs;
        private PersistenceService _persistenceService;
        private PlayerStore _curPlayer;
        
        public void Initialize() {
            SongData[] songData = Resources.LoadAll<SongData>("songs");
            _songs = new SongDictionary();
            _knownSongs = new SongDictionary();
            foreach (SongData song in songData) {
                Debug.Log("Loaded song " + song.name);
                _songs.Add(song.name, new Song(song.beats, song.name));
            }
        }

        private void AddKnownSong(SongData song) {
            _knownSongs.Add(song.name, _songs[song.name]);
        }

        public void PostInitialize() {
            _persistenceService = ServiceLocator.Get<PersistenceService>();
            _persistenceService.OnActivePlayerChanged += OnActivePlayerChanged;
            _curPlayer = _persistenceService.CurrentPlayer;
            _curPlayer.OnSongLearned += OnSongLearned;
            LoadCurPlayerKnownSongs();
        }

        private void LoadCurPlayerKnownSongs() {
            foreach (SongData song in _curPlayer.KnownSongs) {
                AddKnownSong(song);
            }
        }

        private void OnActivePlayerChanged(PlayerStore player) {
            _curPlayer.OnSongLearned -= OnSongLearned;
            player.OnSongLearned += OnSongLearned;
            _curPlayer = player;
            LoadCurPlayerKnownSongs();
        }

        private void OnSongLearned(SongData song) {
            AddKnownSong(song);
        }

        public void Destroy() {
            _songs.Clear();
            _persistenceService.OnActivePlayerChanged -= OnActivePlayerChanged;
        }

        public Song Get(string songName) {
            Song song;
            if (_songs.TryGetValue(songName, out song)) {
                return song;
            }
            throw new Exception("Unknown song with name " + songName + "!");
        }

        public List<Song> CheckSongs(float[] notes) {
            return _knownSongs.Values.Where(song => song.Contains(notes)).ToList();
        }
    }
}