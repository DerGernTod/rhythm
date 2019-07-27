using System;
using System.Runtime.CompilerServices;
using Rhythm.Data;
using Rhythm.Services;
using Rhythm.Songs;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Rhythm.Tutorial {
    [RequireComponent(typeof(CanvasGroup))]
    public class SongTutorial: MonoBehaviour {
        
#pragma warning disable 0649
        [SerializeField] private SongData songData;
        [SerializeField] private Image metronomeDiffIndicator;
        [SerializeField] private RectTransform indicatorLinePrefab; 
#pragma warning restore 0649

        private BeatInputService _beatInputService;
        private GameStateService _gameStateService;
        private CanvasGroup _canvasGroup;
        private Coroutine _canvasGroupFadeCoroutine;
        private Action _update = Constants.Noop;
        private int _metronomeTick = -1;
        private float _indicatorWidth;
        private float _quarterIndicatorWidth;
        private float _lastMetronomeDiff;
        private float _leftOffset;
        private bool _hadBeat;

        private void Start() {
            _beatInputService = ServiceLocator.Get<BeatInputService>();
            _gameStateService = ServiceLocator.Get<GameStateService>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
            _gameStateService.GameStarted += OnGameStarted;
            RectTransform rectTransform = GetComponent<RectTransform>();

            _indicatorWidth = rectTransform.rect.width;
            _quarterIndicatorWidth = _indicatorWidth * .25f;
            _leftOffset = _indicatorWidth * .125f;
            
            float[] beats = songData.beats;
            foreach (float beat in beats) {
                float xPos = _leftOffset + beat * _quarterIndicatorWidth;
                RectTransform indicatorLine = Instantiate(indicatorLinePrefab, transform);
                indicatorLine.anchoredPosition = new Vector2(xPos, indicatorLine.anchoredPosition.y);
            }
        }

        private void OnGameStarted() {
            ServiceLocator.Get<PersistenceService>().CurrentPlayer.LearnSong(songData);
            _beatInputService.OnAfterExecutionStarted += OnAfterExecutionStarted;
            _beatInputService.OnAfterExecutionFinished += OnAfterExecutionFinished;
            _beatInputService.OnBeatLost += OnBeatLost;
            OnBeatLost();
            _update = () => {
                float metronomeDiff = _beatInputService.MetronomeDiff / BeatInputService.NOTE_TIME;
                if (_beatInputService.HasBeat && !_hadBeat) {
                    ResetBeat();
                }
                if (_lastMetronomeDiff < metronomeDiff) {
                    _metronomeTick++;
                }
                float xPos = (_leftOffset + _quarterIndicatorWidth * -metronomeDiff + _metronomeTick * _quarterIndicatorWidth) % _indicatorWidth;
                metronomeDiffIndicator.rectTransform.anchoredPosition = xPos * Vector2.right;
                
                // change color and scale
                float absMetronomeDiff = Mathf.Abs(metronomeDiff);
                if (absMetronomeDiff < BeatInputService.FAIL_TOLERANCE && metronomeDiffIndicator.color != Color.green) {
                    metronomeDiffIndicator.color = Color.green;
                    iTween.ScaleTo(metronomeDiffIndicator.gameObject, Vector3.one + Vector3.up * .15f, BeatInputService.FAIL_TOLERANCE);                        
                } else if (absMetronomeDiff >= BeatInputService.FAIL_TOLERANCE && metronomeDiffIndicator.color != Color.white) {
                    metronomeDiffIndicator.color = Color.white;
                    iTween.ScaleTo(metronomeDiffIndicator.gameObject, Vector3.one, BeatInputService.FAIL_TOLERANCE);
                }

                _lastMetronomeDiff = metronomeDiff;
                
                _hadBeat = _beatInputService.HasBeat;
            };
        }

        private void TriggerFade(float target) {
            if (_canvasGroupFadeCoroutine != null) {
                StopCoroutine(_canvasGroupFadeCoroutine);
            }

            _canvasGroupFadeCoroutine =
                StartCoroutine(Coroutines.FadeTo(_canvasGroup, target, BeatInputService.HALF_NOTE_TIME));
        }

        private void OnBeatLost() {
            _beatInputService.OnNoteHit += OnNoteHit;
            TriggerFade(0);
        }

        private void OnNoteHit(NoteQuality quality, float arg2, int arg3) {
            if (quality != NoteQuality.Miss) {
                ResetBeat();
                TriggerFade(1);
                _beatInputService.OnNoteHit -= OnNoteHit;
            }
        }

        private void ResetBeat() {
            _metronomeTick = 0;
        }

        private void OnAfterExecutionFinished(Song obj) {
            TriggerFade(1);
        }

        private void OnAfterExecutionStarted(Song song) {
            TriggerFade(.25f);
        }

        private void Update() {
            _update();
        }
        private void OnDestroy() {
            _beatInputService.OnAfterExecutionStarted -= OnAfterExecutionStarted;
            _beatInputService.OnAfterExecutionFinished -= OnAfterExecutionFinished;
            _beatInputService.OnExecutionAborted -= OnAfterExecutionFinished;
            _gameStateService.GameStarted -= OnGameStarted;
            _beatInputService.OnNoteHit -= OnNoteHit;
            _beatInputService.OnBeatLost -= OnBeatLost;
        }
    }
}