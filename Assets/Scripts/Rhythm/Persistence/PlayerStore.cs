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
        [field:NonSerialized] public event Action<string> OnSongLearned;
        [field:NonSerialized] public event Action<ItemData> OnItemDiscovered;
        [field:NonSerialized] private ItemDictionary _itemInventory;
        [field:NonSerialized] private List<ItemData> _knownItems;

        private readonly List<string> _knownItemsData;
        private readonly List<string> _itemInventoryData;
        private readonly List<int> _itemInventoryAmount;
        public List<string> KnownSongs { get; }
        public string Name { get; }

        public ItemDictionary ItemInventory => _itemInventory;
        public List<ItemData> KnownItems => _knownItems;

        public PlayerStore(string name) {
            Name = name;
            KnownSongs = new List<string>();
            _itemInventory = new ItemDictionary();
            _itemInventoryData = new List<string>();
            _itemInventoryAmount = new List<int>();
            _knownItems = new List<ItemData>();
            _knownItemsData = new List<string>();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext stream) {
            _itemInventory = new ItemDictionary();
            for (int i = 0; i < _itemInventoryData.Count; i++) {
                _itemInventory[AssetDatabase.LoadAssetAtPath<ItemData>(_itemInventoryData[i])] = _itemInventoryAmount[i];
            }
            _knownItems = new List<ItemData>();
            foreach (string itemDataPath in _knownItemsData) {
                _knownItems.Add(AssetDatabase.LoadAssetAtPath<ItemData>(itemDataPath));
            }
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext stream) {
            _itemInventoryData.Clear();
            _itemInventoryAmount.Clear();
            _knownItemsData.Clear();
            foreach (KeyValuePair<ItemData,int> keyValuePair in _itemInventory) {
                _itemInventoryData.Add(AssetDatabase.GetAssetPath(keyValuePair.Key));
                _itemInventoryAmount.Add(keyValuePair.Value);
            }

            foreach (ItemData knownItem in _knownItems) {
                _knownItemsData.Add(AssetDatabase.GetAssetPath(knownItem));
            }
        }

        public void AddItems(ItemData item, int amount) {
            if (!_knownItems.Contains(item)) {
                OnItemDiscovered?.Invoke(item);
                _knownItems.Add(item);
            }
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

        public bool HasDiscoveredItem(ItemData item) {
            return _knownItems.Contains(item);
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