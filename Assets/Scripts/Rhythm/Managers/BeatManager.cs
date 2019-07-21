using System;
using System.Collections;
using Rhythm.Services;
using Rhythm.Songs;
using Rhythm.Utils;
using UnityEngine;
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

#pragma warning restore 0649
		private class BeatImageDictionary : SerializableDictionary<NoteQuality, Image> {}
		
		private BeatImageDictionary _beatHitImages;
		private Vector2 _latestTouchPosition;
		private Action updateClickLocation;
		private Action _update = Constants.Noop;
		private Vector3 _initialIndicatorPos;
		private bool _isExecutingSong;
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
			_beatHitImages = new BeatImageDictionary {
				{ NoteQuality.Good, hitGood },
				{ NoteQuality.Start, hitGood },
				{ NoteQuality.Miss, hitMiss },
				{ NoteQuality.Perfect, hitNice }
			};
			Action<object> overlayFade = CreateFadeDelegate(beatBlendOverlayImage);
			_overlayFadeHashtable["onupdate"] = overlayFade;
			overlayFade(0f);
			_beatInputService.OnNoteHit += OnNoteHit;
			_beatInputService.OnStreakLost += OnStreakLost;
			_beatInputService.OnAfterExecutionStarted += OnAfterExecutionStarted;
			_beatInputService.OnExecutionAborted += OnExecutionAborted;
			_beatInputService.OnAfterExecutionFinished += OnAfterExecutionFinished;
			updateClickLocation = () => {
				if (Input.touches.Length > 0) {
					_latestTouchPosition = Input.touches[0].position;
				}
				if (Input.GetMouseButtonDown(0)) {
					_latestTouchPosition = Input.mousePosition;
				}
			};
			_gameStateService = ServiceLocator.Get<GameStateService>();
			_gameStateService.GameFinished += OnGameFinished;
			_gameStateService.GameStarted += OnGameStarted;
			_initialIndicatorPos = songIndicator.transform.position;
		}

		private void OnExecutionAborted(Song obj) {
			GameObject songTextGo = songIndicator.gameObject;
			_songIndicatorFadeHashtable["onupdate"] = CreateFadeDelegate(songIndicator);
			songTextGo.transform.position = _initialIndicatorPos;
			iTween.Stop(songTextGo);
			iTween.MoveBy(songTextGo, _moveDownHashtable);
			iTween.ValueTo(songTextGo, _songIndicatorFadeHashtable);
			_beatInputService.OnMetronomeTick -= OnMetronomeTickSongIndicator;
			_isExecutingSong = false;
		}

		private void OnAfterExecutionStarted(Song obj) {
			songIndicator.text = obj.Name.ToUpper();
			_beatInputService.OnMetronomeTick += OnMetronomeTickSongIndicator;
			_isExecutingSong = true;
		}

		private void OnMetronomeTickSongIndicator() {
			GameObject songTextGo = songIndicator.gameObject;
			songTextGo.transform.position = _initialIndicatorPos;
			Color c = songIndicator.color;
			c.a = 1;
			songIndicator.color = c;
			iTween.Stop(songTextGo);
			iTween.PunchPosition(songTextGo, Vector3.up * Screen.height / 20f, BeatInputService.NOTE_TIME * .9f);
		}

		private void OnAfterExecutionFinished(Song obj) {
			GameObject songTextGo = songIndicator.gameObject;
			_songIndicatorFadeHashtable["onupdate"] = CreateFadeDelegate(songIndicator);
			songTextGo.transform.position = _initialIndicatorPos;
			iTween.Stop(songTextGo);
			iTween.MoveBy(songTextGo, _moveDownHashtable);
			iTween.ValueTo(songTextGo, _songIndicatorFadeHashtable);
			_beatInputService.OnMetronomeTick -= OnMetronomeTickSongIndicator;
			_isExecutingSong = false;
		}

		private void OnGameStarted() {
			_update = Constants.Noop;
			_beatInputService.OnMetronomeTick += OnMetronomeTick;
		}
		
		private void OnGameFinished() {
			_update = Constants.Noop;
			_beatInputService.OnMetronomeTick -= OnMetronomeTick;
		}

		private void OnDestroy() {
			_beatInputService.OnNoteHit -= OnNoteHit;
			_gameStateService.GameFinished -= OnGameFinished;
			_gameStateService.GameStarted -= OnGameStarted;
		}

		private void OnNoteHit(NoteQuality quality, float diff, int streak) {
			updateClickLocation();

			Image hitImage;
			switch (quality) {
				case NoteQuality.Bad:
					hitImage = diff > 0 ? hitEarly : hitLate;
					break;
				case NoteQuality.Miss:
				case NoteQuality.Good:
				case NoteQuality.Perfect:
				case NoteQuality.Start:
					hitImage = _beatHitImages[quality];
					break;
				default:
					return;
			}

			_beatHitFadeHashtable["onupdate"] = CreateFadeDelegate(hitImage);
			GameObject hitImageGo = hitImage.gameObject;
			hitImageGo.transform.eulerAngles = Random.value * 90 * Vector3.back;
			Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
			hitImageGo.transform.position = Vector2.Lerp(center, _latestTouchPosition, .5f);
			iTween.PunchScale(hitImageGo, _punchScaleHashtable);
			iTween.ValueTo(hitImageGo, _beatHitFadeHashtable);
		}

		private void OnStreakLost() {
			Image hitImage = _beatHitImages[NoteQuality.Miss];
			_beatHitFadeHashtable["onupdate"] = CreateFadeDelegate(hitImage);
			GameObject hitImageGo = hitImage.gameObject;
			iTween.Stop(hitImageGo);
			Vector2 center = new Vector2(Screen.width / 2f, Screen.height * 3f / 4f);
			hitImageGo.transform.eulerAngles = Vector3.back * 45f;
			hitImageGo.transform.position = Vector2.Lerp(center, _latestTouchPosition, .5f);
			iTween.MoveBy(hitImageGo, _moveDownHashtable);
			iTween.ValueTo(hitImageGo, _beatHitFadeHashtable);
		}

		// Update is called once per frame
		private void Update () {
			_update();
		}

		private void OnMetronomeTick() {
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
