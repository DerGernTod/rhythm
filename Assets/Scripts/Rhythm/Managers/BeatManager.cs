using System;
using System.Collections;
using Rhythm.Services;
using Rhythm.Songs;
using Rhythm.Utils;
using TheNode.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Rhythm.Managers {
	public class BeatManager : MonoBehaviour {
		
#pragma warning disable 0649
		[SerializeField] private AudioClip clipBeat;
		[SerializeField] private Image beatBlendOverlayImage;
		[SerializeField] private Image hitEarly;
		[SerializeField] private Image hitLate;
		[SerializeField] private Image hitMiss;
		[SerializeField] private Image hitNice;
		[SerializeField] private Image hitGood;
		[SerializeField] private Text songIndicator;
		[SerializeField] private AnimatedText streakText;
		[SerializeField] private Gradient[] streakPowerGradients;
#pragma warning restore 0649
		private class BeatImageDictionary : SerializableDictionary<NoteQuality, Image> {}
		
		private BeatImageDictionary _beatHitImages;
		private Vector2 _latestTouchPosition;
		private UnityAction updateClickLocation;
		private UnityAction _update = Constants.Noop;
		private Vector3 _initialIndicatorPos;
		private bool _isExecutingSong;
		private int _prevStreakPower;
		private int _streakPower;
		private readonly Hashtable _overlayFadeHashtable = new Hashtable {
			{ "from", .5f },
			{ "to", 0 }
		};
		private readonly Hashtable _beatHitFadeHashtable = new Hashtable {
			{ "from", 1f },
			{ "to", 0 }
		};
		private readonly Hashtable _songIndicatorFadeHashtable = new Hashtable {
			{ "from", 1f },
			{ "to", 0 }
		};
		private readonly Hashtable _punchScaleHashtable = new Hashtable {
			{ "amount", Vector3.one * .5f },
			{ "time", BeatInputService.NOTE_TIME }
		};
		private readonly Hashtable _moveDownHashtable = new Hashtable {
			{ "y", -500f },
			{ "time", BeatInputService.NOTE_TIME * 2f},
			{ "space", "world" },
			{ "easetype", iTween.EaseType.easeInCirc }
		};
		
		private AudioService _audioService;
		private BeatInputService _beatInputService;
		private GameStateService _gameStateService;

		// Use this for initialization
		private void Start () {
			_audioService = ServiceLocator.Get<AudioService>();
			_beatInputService = ServiceLocator.Get<BeatInputService>();
			_gameStateService = ServiceLocator.Get<GameStateService>();
			_beatHitImages = new BeatImageDictionary {
				{ NoteQuality.Good, hitGood },
				{ NoteQuality.Miss, hitMiss },
				{ NoteQuality.Perfect, hitNice }
			};
			Action<object> overlayFade = CreateFadeDelegate(beatBlendOverlayImage);
			_overlayFadeHashtable["onupdate"] = overlayFade;
			overlayFade(0f);
			_beatInputService.NoteHit += NoteHit;
			_beatInputService.BeatLost += BeatLost;
			_beatInputService.ExecutionStarting += ExecutionStarting;
			_beatInputService.ExecutionStarted += ExecutionStarted;
			_beatInputService.ExecutionAborted += ExecutionAborted;
			_beatInputService.ExecutionFinished += ExecutionFinished;
			_gameStateService.GameFinishing += OnGameFinishing;
			_gameStateService.GameStarted += OnGameStarted;
			updateClickLocation = () => {
				if (Input.touches.Length > 0) {
					_latestTouchPosition = Input.touches[0].position;
				}
				if (Input.GetMouseButtonDown(0)) {
					_latestTouchPosition = Input.mousePosition;
				}
			};
			_initialIndicatorPos = songIndicator.transform.position;
		}

		private void ExecutionStarting(Song song, int streakPower) {
			if (streakPower > _prevStreakPower) {
				StartCoroutine(Coroutines.FadeTo(streakText.GetComponent<CanvasGroup>(), 1,
					BeatInputService.HALF_NOTE_TIME));
				streakText.SetGradient(streakPowerGradients[streakPower - 1]);
				streakText.TriggerImpulse();
			}

			_prevStreakPower = streakPower;
		}

		private void ExecutionAborted(Song obj, int streakPower) {
			GameObject songTextGo = songIndicator.gameObject;
			_songIndicatorFadeHashtable["onupdate"] = CreateFadeDelegate(songIndicator);
			songTextGo.transform.position = _initialIndicatorPos;
			iTween.Stop(songTextGo);
			iTween.MoveBy(songTextGo, _moveDownHashtable);
			iTween.ValueTo(songTextGo, _songIndicatorFadeHashtable);
			_beatInputService.MetronomeTick -= MetronomeTickSongIndicator;
			_isExecutingSong = false;
		}

		private void ExecutionStarted(Song song, int streakPower) {
			songIndicator.text = song.Name.ToUpper();
			_beatInputService.MetronomeTick += MetronomeTickSongIndicator;
			_isExecutingSong = true;
			AudioClip[] clips = song.GetClipsByStreakPower(_streakPower);
			if (clips.Length > 0) {
				_audioService.PlayOneShot(clips[Random.Range(0, clips.Length)]);
			}
		}

		private void MetronomeTickSongIndicator() {
			GameObject songTextGo = songIndicator.gameObject;
			songTextGo.transform.position = _initialIndicatorPos;
			Color c = songIndicator.color;
			c.a = 1;
			songIndicator.color = c;
			iTween.Stop(songTextGo);
			iTween.PunchPosition(songTextGo, Vector3.up * (Screen.height / 20f), BeatInputService.NOTE_TIME * .9f);
		}

		private void ExecutionFinished(Song obj, int streakPower) {
			GameObject songTextGo = songIndicator.gameObject;
			_songIndicatorFadeHashtable["onupdate"] = CreateFadeDelegate(songIndicator);
			songTextGo.transform.position = _initialIndicatorPos;
			iTween.Stop(songTextGo);
			iTween.MoveBy(songTextGo, _moveDownHashtable);
			iTween.ValueTo(songTextGo, _songIndicatorFadeHashtable);
			_beatInputService.MetronomeTick -= MetronomeTickSongIndicator;
			_isExecutingSong = false;
		}

		private void OnGameStarted() {
			_update = Constants.Noop;
			_isExecutingSong = false;
			_prevStreakPower = 0;
			_streakPower = 0;
			_beatInputService.MetronomeTick += MetronomeTick;
			streakText.StartAnimation();
		}
		
		private void OnGameFinishing() {
			_update = Constants.Noop;
			StartCoroutine(Coroutines.FadeTo(
				streakText.GetComponent<CanvasGroup>(),
				0,
				BeatInputService.HALF_NOTE_TIME));
			_beatInputService.MetronomeTick -= MetronomeTick;
			StartCoroutine(Coroutines.FadeTo(
				songIndicator.GetComponent<CanvasGroup>(),
				0,
				BeatInputService.HALF_NOTE_TIME));
		}

		private void OnDestroy() {
			_beatInputService.NoteHit -= NoteHit;
			_gameStateService.GameFinishing -= OnGameFinishing;
			_gameStateService.GameStarted -= OnGameStarted;
			_beatInputService.BeatLost -= BeatLost;
			_beatInputService.ExecutionStarted -= ExecutionStarted;
			_beatInputService.ExecutionAborted -= ExecutionAborted;
			_beatInputService.ExecutionFinished -= ExecutionFinished;
		}

		private void NoteHit(NoteQuality quality, float diff) {
			updateClickLocation();
			Image hitImage;
			if (quality == NoteQuality.Bad) {
				hitImage = diff > 0 ? hitEarly : hitLate;
			} else {
				hitImage = _beatHitImages[quality];
			}
			_beatHitFadeHashtable["onupdate"] = CreateFadeDelegate(hitImage);
			GameObject hitImageGo = hitImage.gameObject;
			hitImageGo.transform.eulerAngles = Random.value * 90 * Vector3.back;
			Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
			hitImageGo.transform.position = Vector2.Lerp(center, _latestTouchPosition, .5f);
			iTween.PunchScale(hitImageGo, _punchScaleHashtable);
			iTween.ValueTo(hitImageGo, _beatHitFadeHashtable);
		}

		private void BeatLost() {
			Image hitImage = _beatHitImages[NoteQuality.Miss];
			_beatHitFadeHashtable["onupdate"] = CreateFadeDelegate(hitImage);
			GameObject hitImageGo = hitImage.gameObject;
			iTween.Stop(hitImageGo);
			Vector2 center = new Vector2(Screen.width / 2f, Screen.height * 3f / 4f);
			hitImageGo.transform.eulerAngles = Vector3.back * 45f;
			hitImageGo.transform.position = Vector2.Lerp(center, _latestTouchPosition, .5f);
			iTween.MoveBy(hitImageGo, _moveDownHashtable);
			iTween.ValueTo(hitImageGo, _beatHitFadeHashtable);
			_streakPower = 0;
			StartCoroutine(Coroutines.FadeTo(streakText.GetComponent<CanvasGroup>(), 0,
				BeatInputService.HALF_NOTE_TIME));
		}

		// Update is called once per frame
		private void Update () {
			_update();
		}

		private void MetronomeTick() {
			_audioService.PlayOneShot(clipBeat, _isExecutingSong ? .15f : 1);
			iTween.ValueTo(gameObject, _overlayFadeHashtable);
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
