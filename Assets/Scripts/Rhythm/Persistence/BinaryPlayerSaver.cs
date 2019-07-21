using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Rhythm.Persistence {
    public static class BinaryPlayerSaver {
        private const string FILE_EXTENSION = ".dat";
        private const string FOLDER_NAME = "PlayerData";
        
        public static void SavePlayer(PlayerStore playerStore) {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            
            string folderPath = Path.Combine(Application.persistentDataPath, FOLDER_NAME);
            if (!Directory.Exists (folderPath))
                Directory.CreateDirectory (folderPath);            

            string dataPath = Path.Combine(folderPath, playerStore.Name + FILE_EXTENSION);
            using (FileStream fs = File.Open(dataPath, FileMode.OpenOrCreate)) {
                binaryFormatter.Serialize(fs, playerStore);
            }
            Debug.Log("Saved player " + playerStore.Name);
        }

        public static List<PlayerStore> LoadPlayers() {
            List<PlayerStore> players = new List<PlayerStore>();
            foreach (string path in GetFilePaths()) {
                BinaryFormatter binaryFormatter = new BinaryFormatter();

                using (FileStream fileStream = File.Open (path, FileMode.Open))
                {
                    players.Add((PlayerStore)binaryFormatter.Deserialize (fileStream));
                }
            }

            return players;
        }
        private static string[] GetFilePaths ()
        {  
            string folderPath = Path.Combine(Application.persistentDataPath, FOLDER_NAME);

            return Directory.GetFiles (folderPath, FILE_EXTENSION);
        }
    }
}