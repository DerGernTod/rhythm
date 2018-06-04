using System;
using System.Collections.Generic;
using UnityEngine;

namespace Services {
    public class ServiceLocator : MonoBehaviour {
        private static Dictionary<Type, IService> _services;
        private void Awake() {
            _services = new Dictionary<Type, IService> {
                { typeof(SongService), new SongService() }
            };

        }

        private static T Get<T>() where T : IService {
            IService service;
            if (_services.TryGetValue(typeof(T), out service)) {
                return (T) service;
            }
            throw new Exception("Service " + typeof(T) + " not registered yet!");
        } 
    }
}