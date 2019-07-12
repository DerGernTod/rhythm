﻿using System;
using System.Collections;
using Rhythm.Services;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Rhythm.Managers {
	public class BeatManager : MonoBehaviour {
		[SerializeField] private AudioClip clipBeat;
		[SerializeField] private Image beatBlendOverlayImage;
		[SerializeField] private Image hitEarly;
		[SerializeField] private Image hitLate;
		[SerializeField] private Image hitMiss;
		[SerializeField] private Image hitNice;
		[SerializeField] private Image hitGood;
		
		private class BeatImageDictionary : SerializableDictionary<BeatQuality, Image> {}
		
		private double _startBeat;
		private int _prevCurBeat;
		private BeatImageDictionary _beatHitImages;
		private Vector2 _latestTouchPosition;
		private Action updateClickLocation;
		
		private readonly Hashtable _overlayFadeHashtable = new Hashtable {
			{ "from", .5f },
			{ "to", 0 }
		};
		private readonly Hashtable _beatHitFadeHashtable = new Hashtable {
			{ "from", 1f },
			{ "to", 0 }
		};

		private readonly Hashtable _punchScaleHashtable = new Hashtable {
			{ "amount", Vector3.one * .5f },
			{ "time", BeatInputService.BEAT_TIME }
		};
		
		private AudioService _audioService;
		
		// Use this for initialization
		private void Start () {
			_startBeat = AudioSettings.dspTime;
			_audioService = ServiceLocator.Get<AudioService>();
			_beatHitImages = new BeatImageDictionary {
				{ BeatQuality.Good, hitGood },
				{ BeatQuality.Start, hitGood },
				{ BeatQuality.Miss, hitMiss },
				{ BeatQuality.Perfect, hitNice }
			};
			Action<object> overlayFade = CreateFadeDelegate(beatBlendOverlayImage);
			_overlayFadeHashtable["onupdate"] = overlayFade;
			overlayFade(0f);
			UpdateBeatsPerSecond(BeatInputService.BEAT_TIME);
			ServiceLocator.Get<BeatInputService>().BeatHit += BeatHit;
			updateClickLocation = () => {
				if (Input.touches.Length > 0) {
					_latestTouchPosition = Input.touches[0].position;
				}
				if (Input.GetMouseButtonDown(0)) {
					_latestTouchPosition = Input.mousePosition;
				}
			};
		}

		private void OnDestroy() {
			ServiceLocator.Get<BeatInputService>().BeatHit -= BeatHit;
		}

		private void BeatHit(BeatQuality quality, float diff, int streak) {
			updateClickLocation();
			if (quality == BeatQuality.Start && streak == 0) {
				UpdateBeatsPerSecond(BeatInputService.BEAT_TIME);
			}

			Image hitImage;
			switch (quality) {
				case BeatQuality.Bad:
					hitImage = diff > 0 ? hitEarly : hitLate;
					break;
				case BeatQuality.Miss:
				case BeatQuality.Good:
				case BeatQuality.Perfect:
				case BeatQuality.Start:
					hitImage = _beatHitImages[quality];
					break;
				default:
					return;
			}

			_beatHitFadeHashtable["onupdate"] = CreateFadeDelegate(hitImage);
			GameObject hitImageGo = hitImage.gameObject;
			hitImageGo.transform.eulerAngles = Vector3.back * UnityEngine.Random.value * 90;
			Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
			hitImageGo.transform.position = Vector2.Lerp(center, _latestTouchPosition, .5f);
			iTween.PunchScale(hitImageGo, _punchScaleHashtable);
			iTween.ValueTo(hitImageGo, _beatHitFadeHashtable);
		}

		// Update is called once per frame
		private void Update () {
			int curBeat = (int) Math.Floor((AudioSettings.dspTime - _startBeat) / BeatInputService.BEAT_TIME);
			if (!(curBeat > _prevCurBeat + BeatInputService.BEAT_TIME)) return;
			TriggerBeat(curBeat);
		}

		private void TriggerBeat(int prevBeat) {
			_audioService.PlayOneShot(clipBeat);
			iTween.ValueTo(gameObject, _overlayFadeHashtable);
			_prevCurBeat = prevBeat;
		}

		private void UpdateBeatsPerSecond(float bps) {
			_startBeat = AudioSettings.dspTime - BeatInputService.BEAT_TIME;
			_overlayFadeHashtable["time"] = bps * .9f;
			TriggerBeat((int) Math.Floor((AudioSettings.dspTime - _startBeat) / BeatInputService.BEAT_TIME));
		}
		
		private static Action<object> CreateFadeDelegate(Graphic target) {
			return alpha => {
				Color c = target.color;
				c.a = (float) alpha;
				target.color = c;
			};
		}
	}
}