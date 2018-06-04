using System;
using System.Collections;
using JetBrains.Annotations;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace Managers {
	public class BeatManager : MonoBehaviour {
		[SerializeField] private AudioClip _clipBeat;
		[SerializeField] private Image _beatBlendOverlayImage;
		
		private double _startBeat;
		private int _prevCurBeat;
		private readonly Hashtable _overlayFadeHashtable = new Hashtable {
			{ "from", .5f },
			{ "to", 0 },
			{ "onupdate", "UpdateOverlayAlpha" }
		};
		
		// Use this for initialization
		private void Start () {
			_startBeat = AudioSettings.dspTime;
			UpdateOverlayAlpha(0);
			UpdateBeatsPerSecond(BeatInputService.BeatTime);
			ServiceLocator.Get<BeatInputService>().OnBeatHit += quality => {
				if (quality == BeatInputService.Quality.Start) {
					UpdateBeatsPerSecond(BeatInputService.BeatTime);
				}
			};
		}
	
		// Update is called once per frame
		private void Update () {
			int curBeat = (int) Math.Floor((AudioSettings.dspTime - _startBeat) / BeatInputService.BeatTime);
			if (!(curBeat > _prevCurBeat + BeatInputService.BeatTime)) return;
			// GetComponent<AudioSource>().PlayOneShot(_clipBeat);
			iTween.ValueTo(gameObject, _overlayFadeHashtable);
			_prevCurBeat = curBeat;
		}

		[UsedImplicitly]
		private void UpdateOverlayAlpha(float alpha) {
			Color c = _beatBlendOverlayImage.color;
			c.a = alpha;
			_beatBlendOverlayImage.color = c;
		}
		
		public void UpdateBeatsPerSecond(float bps) {
			_overlayFadeHashtable["time"] = bps * .9f;
			_prevCurBeat = (int) Math.Floor((AudioSettings.dspTime - _startBeat) / BeatInputService.BeatTime);
		}
	}
}
