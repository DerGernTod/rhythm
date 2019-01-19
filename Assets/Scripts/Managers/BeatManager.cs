using System;
using System.Collections;
using JetBrains.Annotations;
using Services;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649
namespace Managers {
	public class BeatManager : MonoBehaviour {
		[SerializeField] private AudioClip clipBeat;
		[SerializeField] private Image beatBlendOverlayImage;
		
		private double _startBeat;
		private int _prevCurBeat;
		private readonly Hashtable _overlayFadeHashtable = new Hashtable {
			{ "from", .5f },
			{ "to", 0 },
			{ "onupdate", "UpdateOverlayAlpha" }
		};

		private AudioService _audioService;
		
		// Use this for initialization
		private void Start () {
			_startBeat = AudioSettings.dspTime;
			_audioService = ServiceLocator.Get<AudioService>();
			UpdateOverlayAlpha(0);
			UpdateBeatsPerSecond(BeatInputService.BeatTime);
			ServiceLocator.Get<BeatInputService>().BeatHit += BeatHit;
		}

		private void OnDestroy() {
			ServiceLocator.Get<BeatInputService>().BeatHit -= BeatHit;
		}

		private void BeatHit(BeatQuality quality, float diff, int streak) {
			if (quality == BeatQuality.Start && streak == 0) {
				// UpdateBeatsPerSecond(BeatInputService.BeatTime);
			}
		}

		// Update is called once per frame
		private void Update () {
			int curBeat = (int) Math.Floor((AudioSettings.dspTime - _startBeat) / BeatInputService.BeatTime);
			if (!(curBeat > _prevCurBeat + BeatInputService.BeatTime)) return;
			TriggerBeat(curBeat);
			
		}

		private void TriggerBeat(int prevBeat) {
			_audioService.PlayOneShot(clipBeat);
			iTween.ValueTo(gameObject, _overlayFadeHashtable);
			_prevCurBeat = prevBeat;
		}

		[UsedImplicitly]
		private void UpdateOverlayAlpha(float alpha) {
			Color c = beatBlendOverlayImage.color;
			c.a = alpha;
			beatBlendOverlayImage.color = c;
		}
		
		public void UpdateBeatsPerSecond(float bps) {
			_startBeat = AudioSettings.dspTime - BeatInputService.BeatTime;
			_overlayFadeHashtable["time"] = bps * .9f;
			TriggerBeat((int) Math.Floor((AudioSettings.dspTime - _startBeat) / BeatInputService.BeatTime));
		}
	}
}
