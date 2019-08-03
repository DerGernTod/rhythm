using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Rhythm.Persistence {
    public static class BinaryPlayerSaver {
        private const string FILE_EXTENSION = ".dat";
        private const string FOLDER_NAME = "PlayerData";

        public static void DeletePlayer(PlayerStore playerStore) {
            string playerStoreFolderPath = GetPlayerStoreFolderPath();
            string playerStorePath = GetPlayerStorePath(playerStore, playerStoreFolderPath);
            File.Delete(playerStorePath);
            Debug.Log("Deleted player " + playerStore.Name);
        }
        
        public static void SavePlayer(PlayerStore playerStore) {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            
            string folderPath = GetPlayerStoreFolderPath();
            if (!Directory.Exists (folderPath))
                Directory.CreateDirectory (folderPath);            

            string dataPath = GetPlayerStorePath(playerStore, folderPath);
            using (FileStream fs = File.Open(dataPath, FileMode.OpenOrCreate)) {
                binaryFormatter.Serialize(fs, playerStore);
            }
            Debug.Log("Saved player " + playerStore.Name + " to " + dataPath);
        }

        private static string GetPlayerStorePath(PlayerStore playerStore, string folderPath) {
            return Path.Combine(folderPath, playerStore.Name + FILE_EXTENSION);
        }

        private static string GetPlayerStoreFolderPath() {
            return Path.Combine(Application.persistentDataPath, FOLDER_NAME);
        }

        public static List<PlayerStore> LoadPlayers() {
            List<PlayerStore> players = new List<PlayerStore>();
            foreach (string path in GetFilePaths()) {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                
                Debug.Log("Loading player from path " + path);
                using (FileStream fileStream = File.Open (path, FileMode.Open))
                {
                    players.Add((PlayerStore)binaryFormatter.Deserialize (fileStream));
                }
            }

            return players;
        }
        private static string[] GetFilePaths() {  
            return Directory.GetFiles (GetPlayerStoreFolderPath(), "*" + FILE_EXTENSION);
        }
    }
}