using System;
using Rhythm.Services;
using Rhythm.Songs;
using Rhythm.UI;
using Rhythm.Utils;
using UnityEngine;
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
        
        // todo: how to tell user to wait after a full beat for the units to work?
        // maybe pause? or different sound for main drum, disable drum, different color for border?
        // timeline with metronome spots marked?
        // on successful song, indicate pause
        private void Start() {
            _beatInputService = ServiceLocator.Get<BeatInputService>();
            _gameStateService = ServiceLocator.Get<GameStateService>();
        }

        public void OnComicPageCompleted(int page) {
            if (page == 1) {
                _gameStateService.TriggerGameStarted();
                iTween.ScaleBy(pageToScale.gameObject, Vector3.one * 1.05f, 5);
                _beatInputService.OnMetronomeTick += PunchDrum;
                _beatInputService.OnNoteHit += OnNoteHit;
                _beatInputService.OnStreakLost += OnStreakLost;
                _beatInputService.OnAfterExecutionStarted += OnAfterExecutionStarted;
                _beatInputService.OnAfterExecutionFinished += OnAfterExecutionFinished;
                _beatInputService.OnExecutionAborted += OnAfterExecutionFinished;
            }
        }

        private void OnMetronomeTick() {
            iTween.PunchPosition(mood.gameObject, Vector2.up * 10, BeatInputService.NOTE_TIME * .9f);
            if (_drumIndex >= 0) {
                drum.sprite = drumSprites[_drumIndex--];
            }
        }

        private void OnAfterExecutionStarted(Song song) {
            _isExecutingSong = true;
            _drumIndex = 3;
            _beatInputService.OnMetronomeTick += OnMetronomeTick;
            
        }

        private void OnAfterExecutionFinished(Song song) {
            _isExecutingSong = false;
            _beatInputService.OnMetronomeTick -= OnMetronomeTick;
            
        }

        private void OnStreakLost() {
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
                        // FinishTutorial();
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
            _gameStateService.TriggerGameFinished();
        }
        
        private void SetMoodSpriteIndex(int index) {
            _moodIndex = Mathf.Clamp(index, 0, moodSprites.Length - 1);
            mood.sprite = moodSprites[_moodIndex];
        }

        private void OnDestroy() {
            _beatInputService.OnMetronomeTick -= PunchDrum;
            _beatInputService.OnNoteHit -= OnNoteHit;
            _beatInputService.OnStreakLost -= OnStreakLost;
            _beatInputService.OnAfterExecutionStarted -= OnAfterExecutionStarted;
            _beatInputService.OnAfterExecutionFinished -= OnAfterExecutionFinished;
            _beatInputService.OnExecutionAborted -= OnAfterExecutionFinished;
        }

        private void PunchDrum() {
            if (!_isExecutingSong) {
                iTween.PunchScale(drum.gameObject, Vector2.one * .25f, BeatInputService.NOTE_TIME * .9f);
                iTween.PunchRotation(drum.gameObject, 15f * (2 * Random.value - 1) * Vector3.forward,
                    BeatInputService.NOTE_TIME * .9f);
            }
        }

    }
}