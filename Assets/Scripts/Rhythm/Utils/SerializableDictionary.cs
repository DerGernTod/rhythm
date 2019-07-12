using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace Rhythm.Utils {
    [Serializable]
    public abstract class SerializableDictionary<TK, TV> : ISerializationCallbackReceiver, IEnumerable<KeyValuePair<TK, TV>> {
        private readonly Object _mutexForDictionary = new Object();
        private readonly Dictionary<TK, TV> _dictionary = new Dictionary<TK, TV>();

        [SerializeField] private readonly List<TK> _listOfKeys = new List<TK>();
        [SerializeField] private readonly List<TV> _listOfValues = new List<TV>();

        #region Serialization Related

        public void OnBeforeSerialize() {
            lock (_mutexForDictionary) {
                _listOfKeys.Clear();
                _listOfValues.Clear();

                foreach (KeyValuePair<TK, TV> eachPair in _dictionary) {
                    _listOfKeys.Add(eachPair.Key);
                    _listOfValues.Add(eachPair.Value);
                }
            }
        }

        public void OnAfterDeserialize() {
            lock (_mutexForDictionary) {
                _dictionary.Clear();
                CheckIfKeyAndValueValid();

                for (int i = 0; i < _listOfKeys.Count; ++i) {
                    _dictionary.Add(_listOfKeys[i], _listOfValues[i]);
                }
            }
        }

        #endregion

        #region Dictionary Interface

        public void Add(TK key, TV value) {
            lock (_mutexForDictionary) {
                _dictionary.Add(key, value);
            }
        }

        public TV this[TK key] {
            get {
                lock (_mutexForDictionary) {
                    return _dictionary[key];
                }
            }
            set {
                lock (_mutexForDictionary) {
                    _dictionary[key] = value;
                }
            }
        }

        public void Remove(TK key) {
            lock (_mutexForDictionary) {
                _dictionary.Remove(key);
            }
        }

        public int Count {
            get {
                lock (_mutexForDictionary) {
                    return _dictionary.Count;
                }
            }
        }

        public void Clear() {
            lock (_mutexForDictionary) {
                _dictionary.Clear();
            }
        }
        
        public Dictionary<TK, TV>.ValueCollection Values {
            get {
                lock (_mutexForDictionary) {
                    return _dictionary.Values;
                }
            }
        }

        public bool TryGetValue(TK key, out TV value) {
            lock (_mutexForDictionary) {
                return _dictionary.TryGetValue(key, out value);
            }
        }

        #endregion

        private void CheckIfKeyAndValueValid() {
            int numberOfKeys = _listOfKeys.Count;
            int numberOfValues = _listOfValues.Count;

            if (numberOfKeys != numberOfValues) {
                throw new ArgumentException("(nKey, nValue) = ("
                                                   + numberOfKeys + ", "
                                                   + numberOfValues + ") are NOT Equal!");
            }
        }

        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator() {
            lock (_mutexForDictionary) {
                return _dictionary.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    } //End of class
}