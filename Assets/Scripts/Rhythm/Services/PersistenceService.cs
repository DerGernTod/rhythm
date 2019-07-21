using System;
using System.Collections.Generic;
using Rhythm.Persistence;
using UnityEngine;

namespace Rhythm.Services {
    public class PersistenceService: IService {
        public event Action<PlayerStore> OnActivePlayerChanged;
        public List<PlayerStore> Players { get; private set; }
        public PlayerStore CurrentPlayer { get; private set; }
        
        public void Initialize() {
            try {
                Players = BinaryPlayerSaver.LoadPlayers();
            } catch (Exception e) {
                Debug.Log("Error loading players: " + e.Message);
                Players = new List<PlayerStore>();
            }

            if (Players.Count == 0) {
                Players.Add(new PlayerStore("default"));
            }
            ChangeActivePlayer(Players[0]);
        }

        public void AddPlayer(string name) {
            PlayerStore player = new PlayerStore(name);
            Players.Add(player);
            CurrentPlayer = player;
            BinaryPlayerSaver.SavePlayer(player);
        }

        public void SaveCurrentPlayer() {
            BinaryPlayerSaver.SavePlayer(CurrentPlayer);
        }

        public void ChangeActivePlayer(PlayerStore player) {
            CurrentPlayer = player;
            OnActivePlayerChanged?.Invoke(CurrentPlayer);
        }

        public void PostInitialize() {
        }

        public void Destroy() {
        }
    }
}