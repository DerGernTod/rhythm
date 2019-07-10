using UnityEngine;

namespace Rhythm.Services {
	public class AudioService : IService {
		private readonly AudioSource _provider;
		public AudioService(AudioSource provider) {
			_provider = provider;
		}

		public void PlayOneShot(AudioClip clip) {
			_provider.PlayOneShot(clip);
		}
		
		public void Initialize() {
		}

		public void PostInitialize() {
		}

		public void Destroy() {
			
		}
	}
}
