using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;

namespace Services {
    public class ServiceLocator : MonoBehaviour {
        private static Dictionary<Type, IService> _services;
        private IUpdateableService[] _updateableServices;
        private void Awake() {
            BeatInputService beatInputService = new BeatInputService(this);
            _services = new Dictionary<Type, IService> {
                { typeof(SongService), new SongService() },
                { typeof(BeatInputService), beatInputService }
            };
            _updateableServices = new IUpdateableService[] {
                beatInputService
            };

            foreach (IService service in _services.Values) {
                service.Initialize();
            }
            
            foreach (IService service in _services.Values) {
                service.PostInitialize();
            }
        }

        private void Update() {
            foreach (IUpdateableService service in _updateableServices) {
                service.Update(Time.deltaTime);
            }
        }

        public static T Get<T>() where T : IService {
            IService service;
            if (_services.TryGetValue(typeof(T), out service)) {
                return (T) service;
            }
            throw new Exception("Service " + typeof(T) + " not registered yet!");
        } 
    }
}