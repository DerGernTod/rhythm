using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Managers {
	public class BeatManager : MonoBehaviour {
		[SerializeField] private float _beatsPerSecond = .75f;
		[SerializeField] private AudioClip _clipBeat;
		[SerializeField] private Image _beatBlendOverlayImage;
		
		private double _startBeat;
		private int _prevCurBeat;
		private readonly Hashtable _overlayFadeHashtable = new Hashtable {
			{ "from", 1 },
			{ "to", 0 },
			{ "onupdate", "UpdateOverlayAlpha" }
		};
		
		// Use this for initialization
		private void Start () {
			_startBeat = AudioSettings.dspTime;
			UpdateOverlayAlpha(0);
			UpdateBeatsPerSecond(_beatsPerSecond);
		}
	
		// Update is called once per frame
		private void Update () {
			int curBeat = (int) Math.Floor((AudioSettings.dspTime - _startBeat) / _beatsPerSecond);
			if (!(curBeat > _prevCurBeat + _beatsPerSecond)) return;
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
			_beatsPerSecond = bps;
			_overlayFadeHashtable["time"] = bps * .9f;
			_prevCurBeat = (int) Math.Floor((AudioSettings.dspTime - _startBeat) / _beatsPerSecond);
		}

		public float GetBeatsPerSecond() {
			return _beatsPerSecond;
		}
	}
}
