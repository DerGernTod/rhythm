using System;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Rhythm.Services {
    [RequireComponent(typeof(AudioSource))]
    public class ServiceLocator : MonoBehaviour {
        private static ServiceDictionary services;
        [FormerlySerializedAs("_updateableServices")] [SerializeField]
        private IUpdateableService[] updateableServices;
        private void Awake() {
            BeatInputService beatInputService = new BeatInputService(this);
            services = new ServiceDictionary {
                { typeof(SongService), new SongService() },
                { typeof(BeatInputService), beatInputService },
                { typeof(AudioService), new AudioService(GetComponent<AudioSource>())},
                { typeof(UnitService), new UnitService() },
                { typeof(GameStateService), new GameStateService() }
            };
            updateableServices = new IUpdateableService[] {
                beatInputService
            }; 

            foreach (IService service in services.Values) {
                service.Initialize();
            }
            
            foreach (IService service in services.Values) {
                service.PostInitialize();
            }
        }

        private void Update() {
            foreach (IUpdateableService service in updateableServices) {
                service.Update(Time.deltaTime);
            }
        }

        private void OnDestroy() {
            foreach (IUpdateableService service in updateableServices) {
                service.Destroy();
            }
        }

        public static T Get<T>() where T : IService {
            IService service;
            if (services.TryGetValue(typeof(T), out service)) {
                return (T) service;
            }
            throw new Exception("Service " + typeof(T) + " not registered yet!"); 
        } 
    }
}