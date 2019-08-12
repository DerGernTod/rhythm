using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Rhythm.Data;
using Rhythm.Utils;
using UnityEditor;
using UnityEngine;

namespace Rhythm.Persistence {
    public class ItemDictionary : SerializableDictionary<ItemData, int> { }
    
    [Serializable]
    public class PlayerStore {
        [field:NonSerialized]
        public event Action<string> OnSongLearned;
        [field:NonSerialized]
        private ItemDictionary _itemInventory;

        private readonly List<string> _itemInventoryData;
        private readonly List<int> _itemInventoryAmount;
        public List<string> KnownSongs { get; }
        public string Name { get; }

        public ItemDictionary ItemInventory => _itemInventory;

        public PlayerStore(string name) {
            Name = name;
            KnownSongs = new List<string>();
            _itemInventory = new ItemDictionary();
            _itemInventoryData = new List<string>();
            _itemInventoryAmount = new List<int>();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext stream) {
            _itemInventory = new ItemDictionary();
            for (int i = 0; i < _itemInventoryData.Count; i++) {
                _itemInventory[AssetDatabase.LoadAssetAtPath<ItemData>(_itemInventoryData[i])] = _itemInventoryAmount[i];
            }
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext stream) {
            _itemInventoryData.Clear();
            _itemInventoryAmount.Clear();
            foreach (KeyValuePair<ItemData,int> keyValuePair in _itemInventory) {
                _itemInventoryData.Add(AssetDatabase.GetAssetPath(keyValuePair.Key));
                _itemInventoryAmount.Add(keyValuePair.Value);
            }
        }

        public void AddItems(ItemData item, int amount) {
            int curAmount;
            ItemInventory.TryGetValue(item, out curAmount);
            curAmount += amount;
            ItemInventory[item] = curAmount;
        }

        public void RemoveItems(ItemData item, int amount) {
            int curAmount;
            bool hasValue = ItemInventory.TryGetValue(item, out curAmount);
            curAmount = Mathf.Max(0, curAmount - amount);
            if (hasValue) {
                if (curAmount == 0) {
                    ItemInventory.Remove(item);
                } else {
                    ItemInventory[item] = curAmount;
                }
            }
        }

        public int GetItemAmount(ItemData item) {
            int curAmount;
            return ItemInventory.TryGetValue(item, out curAmount) ? curAmount : 0;
        }

        public bool HasItems(ItemData item, int amount) {
            int curAmount;
            ItemInventory.TryGetValue(item, out curAmount);
            return curAmount >= amount;
        }

        public void LearnSong(string songName) {
            if (!KnownSongs.Contains(songName)) {
                KnownSongs.Add(songName);
                OnSongLearned?.Invoke(songName);
            }
        }
    }
}