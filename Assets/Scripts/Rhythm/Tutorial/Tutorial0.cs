using System;
using Rhythm.Services;
using Rhythm.Songs;
using Rhythm.UI;
using Rhythm.Utils;
using TheNode.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Rhythm.Tutorial {
    public class Tutorial0 : MonoBehaviour {
        
#pragma warning disable 0649
        [SerializeField] private ComicPage pageToScale;
        [SerializeField] private Image drum;
        [SerializeField] private Image mood;
        [SerializeField] private Sprite[] moodSprites;
        [SerializeField] private Sprite[] drumSprites;
        [SerializeField] private AnimatedText songLearnedText;
#pragma warning restore 0649
        private BeatInputService _beatInputService;
        private bool _moodDirection;
        private int _moodIndex;
        private const int FAIL_TOLERANCE = 5;
        private int _failCount;
        private int _successCount;
        private bool _isExecutingSong;
        private GameStateService _gameStateService;
        private int _drumIndex;
        private Action _update = Constants.Noop;
        
        private void Start() {
            _beatInputService = ServiceLocator.Get<BeatInputService>();
            _gameStateService = ServiceLocator.Get<GameStateService>();
        }

        private void Update() {
            _update();
        }

        public void OnComicPageCompleted(int page) {
            if (page == 1) {
                _gameStateService.TriggerGameStarted();
                iTween.ScaleBy(pageToScale.gameObject, Vector3.one * 1.05f, 5);
                _beatInputService.MetronomeTick += OnMetronomeTick;
                _beatInputService.NoteHit += OnNoteHit;
                _beatInputService.BeatLost += OnBeatLost;
                _beatInputService.ExecutionStarted += OnExecutionStarted;
                _beatInputService.ExecutionFinished += OnExecutionFinished;
                _beatInputService.ExecutionAborted += OnExecutionFinished;
            }
        }

        private void MetronomeTick() {
            iTween.PunchPosition(mood.gameObject, Vector2.up * 10, BeatInputService.NOTE_TIME * .9f);
            if (_drumIndex >= 0) {
                drum.sprite = drumSprites[_drumIndex--];
            }
        }

        private void OnExecutionStarted(Song song) {
            _isExecutingSong = true;
            _drumIndex = 3;
            _beatInputService.MetronomeTick += MetronomeTick;
            
        }

        private void OnExecutionFinished(Song song) {
            _isExecutingSong = false;
            _beatInputService.MetronomeTick -= MetronomeTick;
            
        }

        private void OnBeatLost() {
            HandleFail();
        }

        private void OnNoteHit(NoteQuality quality, float diff, int streak) {
            if (quality != NoteQuality.Miss) {
                iTween.PunchRotation(mood.gameObject, 30f * (_moodDirection ? 1 : -1) * Vector3.forward, BeatInputService.NOTE_TIME * .9f);
                _moodDirection = !_moodDirection;
                _successCount++;
                if (_successCount >= 4) {
                    _successCount = 0;
                    SetMoodSpriteIndex(_moodIndex + 1);
                    if (streak == 6) {
                        FinishTutorial();
                    }
                }
            } else {
                HandleFail();
            }
        }

        private void HandleFail() {
            _successCount = Mathf.Max(0, _successCount - 1);
            _failCount++;
            if (_failCount >= FAIL_TOLERANCE) {
                _failCount = 0;
                SetMoodSpriteIndex(_moodIndex - 1);
            }
        }

        private void FinishTutorial() {
            OnDestroy();
            _gameStateService.TriggerGameFinishing();
            StartCoroutine(Coroutines.FadeTo(songLearnedText.GetComponent<CanvasGroup>(), 1, .25f));
            songLearnedText.StartAnimation();
            songLearnedText.TriggerImpulse();
            StartCoroutine(Coroutines.ExecuteAfterSeconds(1.5f, () => {
                _update = () => {
                    if (Input.GetMouseButtonDown(0)) {
                        _update = Constants.Noop;
                        _gameStateService.TriggerSceneTransition("IngameScene");
                    }
                };
            }));
            
        }
        
        private void SetMoodSpriteIndex(int index) {
            _moodIndex = Mathf.Clamp(index, 0, moodSprites.Length - 1);
            mood.sprite = moodSprites[_moodIndex];
        }

        private void OnDestroy() {
            _beatInputService.MetronomeTick -= OnMetronomeTick;
            _beatInputService.NoteHit -= OnNoteHit;
            _beatInputService.BeatLost -= OnBeatLost;
            _beatInputService.ExecutionStarted -= OnExecutionStarted;
            _beatInputService.ExecutionFinished -= OnExecutionFinished;
            _beatInputService.ExecutionAborted -= OnExecutionFinished;
        }

        private void OnMetronomeTick() {
            if (!_isExecutingSong) {
                iTween.PunchScale(drum.gameObject, Vector2.one * .25f, BeatInputService.NOTE_TIME * .9f);
                iTween.PunchRotation(drum.gameObject, 15f * (2 * Random.value - 1) * Vector3.forward,
                    BeatInputService.NOTE_TIME * .9f);
            }
        }

    }
}