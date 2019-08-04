using System;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Rhythm.Services {
    [RequireComponent(typeof(AudioSource))]
    public class ServiceLocator : MonoBehaviour {
        private static ServiceDictionary services;
        
#pragma warning disable 0649
        [FormerlySerializedAs("_updateableServices")] [SerializeField]
        private IUpdateableService[] updateableServices;

        [FormerlySerializedAs("startScene")] [SerializeField] private BuildScenes startBuildScene;
#pragma warning restore 0649
        
        private void Awake() {
            BeatInputService beatInputService = new BeatInputService(this);
            services = new ServiceDictionary {
                { typeof(SongService), new SongService() },
                { typeof(BeatInputService), beatInputService },
                { typeof(AudioService), new AudioService(GetComponent<AudioSource>())},
                { typeof(UnitService), new UnitService() },
                { typeof(GameStateService), new GameStateService() },
                { typeof(PersistenceService), new PersistenceService() }
            };
            updateableServices = new IUpdateableService[] {
                beatInputService
            }; 
            VerifyBuildOrder();

            foreach (IService service in services.Values) {
                service.Initialize();
            }
            
            foreach (IService service in services.Values) {
                service.PostInitialize();
            }
        }

        private void VerifyBuildOrder() {
            Array sceneIndices = Enum.GetValues(typeof(BuildScenes));
            int errorCount = 0;
            foreach (object sceneIndex in sceneIndices) {
                string sceneName = Enum.GetName(typeof(BuildScenes), sceneIndex);
                string scenePath = SceneUtility.GetScenePathByBuildIndex((int)sceneIndex);
                // ReSharper disable once PossibleNullReferenceException
                if (!scenePath.Contains(sceneName)) {
                    Debug.LogError("Build index '" + sceneIndex + "' of scene doesn't match! Expected " + sceneName + " to be part of path " + scenePath);
                    errorCount++;
                    Application.Quit(-1);
                }
            }

            if (errorCount > 0) {
                Debug.Break();
            }
        }

        private void Start() {
            Get<GameStateService>().TriggerSceneTransition(startBuildScene);
        }

        private void Update() {
            foreach (IUpdateableService service in updateableServices) {
                service.Update(Time.deltaTime);
            }
        }

        private void FixedUpdate() {
            foreach (IUpdateableService service in updateableServices) {
                service.FixedUpdate();
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