using System;
using Managers;
using UnityEngine;
using Utils;

namespace Services {
    [RequireComponent(typeof(AudioSource))]
    public class ServiceLocator : MonoBehaviour {
        [SerializeField]
        private static ServiceDictionary _services;
        [SerializeField]
        private IUpdateableService[] _updateableServices;
        private void Awake() {
            BeatInputService beatInputService = new BeatInputService(this);
            _services = new ServiceDictionary {
                { typeof(SongService), new SongService() },
                { typeof(BeatInputService), beatInputService },
                { typeof(AudioService), new AudioService(GetComponent<AudioSource>())},
                { typeof(UnitService), new UnitService() }
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

        private void OnDestroy() {
            foreach (IUpdateableService service in _updateableServices) {
                service.Destroy();
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