using System;
using System.Collections.Generic;
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
        private int _metronomeTick = 2;
        private float _indicatorWidth;
        private float _quarterIndicatorWidth;
        private float _lastMetronomeDiff;
        private bool _hadBeat;
        private List<RectTransform> _indicatorLines;
        private List<CanvasGroup> _lineCanvasGroups;
        private float _halfIndicatorWidth;

        private void Start() {
            _beatInputService = ServiceLocator.Get<BeatInputService>();
            _gameStateService = ServiceLocator.Get<GameStateService>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
            _gameStateService.GameStarted += OnGameStarted;
            RectTransform rectTransform = GetComponent<RectTransform>();

            _indicatorWidth = rectTransform.rect.width;
            _halfIndicatorWidth = _indicatorWidth * .5f;
            _quarterIndicatorWidth = _indicatorWidth * .25f;
            _indicatorLines = new List<RectTransform>();
            _lineCanvasGroups = new List<CanvasGroup>();
            float[] beats = songData.beats;
            foreach (float beat in beats) {
                float xPos = beat * _quarterIndicatorWidth;
                RectTransform indicatorLine = Instantiate(indicatorLinePrefab, transform);
                indicatorLine.anchoredPosition = new Vector2(xPos, indicatorLine.anchoredPosition.y);
                _indicatorLines.Add(indicatorLine);
                _lineCanvasGroups.Add(indicatorLine.GetComponent<CanvasGroup>());
            }
            CalcIndicatorPosition(_beatInputService.MetronomeDiff / BeatInputService.NOTE_TIME);
            
        }

        private void OnGameStarted() {
            ServiceLocator.Get<PersistenceService>().CurrentPlayer.LearnSong(songData);
            _beatInputService.ExecutionStarted += ExecutionStarted;
            _beatInputService.ExecutionFinished += ExecutionFinished;
            _beatInputService.BeatLost += BeatLost;
            _beatInputService.NoteHit += PunchClosestIndicator;
            BeatLost();
            _update = UpdateTutorial;
            TriggerFade(1);
        }

        private void PunchClosestIndicator(NoteQuality noteQuality, float diff, int streak) {
            if (noteQuality == NoteQuality.Miss) {
                return;
            }
            int closestIndex = 0;
            // if we don't have a beat, we reset and thus always punch the first index
            if (_hadBeat) {
                float closestDist = 1;
                for (int i = 0; i < _indicatorLines.Count; i++) {
                    float curDist = CalcDistToIndicator(_indicatorLines[i]);
                    if (closestDist > curDist) {
                        closestIndex = i;
                        closestDist = curDist;
                    }
                }
            }
            iTween.PunchScale(_indicatorLines[closestIndex].gameObject, Vector3.one * 1.1f, BeatInputService.NOTE_TIME);
        }

        private void UpdateTutorial() {
            float metronomeDiff = _beatInputService.MetronomeDiff / BeatInputService.NOTE_TIME;
            if (_beatInputService.HasBeat && !_hadBeat) {
                ResetBeat();
            }

            if (_lastMetronomeDiff < metronomeDiff) {
                _metronomeTick++;
            }

            CalcIndicatorPosition(metronomeDiff);

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
        }

        private void CalcIndicatorPosition(float metronomeDiff) {
            float xPos = _quarterIndicatorWidth * -metronomeDiff + _metronomeTick * _quarterIndicatorWidth;
            for (int i = 0; i < _indicatorLines.Count; i++) {
                _indicatorLines[i].anchoredPosition =
                    ((songData.beats[i] * _quarterIndicatorWidth - xPos) % _indicatorWidth + _indicatorWidth) * Vector2.right;
                float distToIndicator = CalcDistToIndicator(_indicatorLines[i].transform);
                _lineCanvasGroups[i].alpha = 1 - distToIndicator * distToIndicator;
            }
        }

        private float CalcDistToIndicator(Transform targetTransform) {
            return Mathf.Abs(targetTransform.position.x - metronomeDiffIndicator.transform.position.x) /
                   _halfIndicatorWidth;
        }

        private void TriggerFade(float target) {
            if (_canvasGroupFadeCoroutine != null) {
                StopCoroutine(_canvasGroupFadeCoroutine);
            }

            _canvasGroupFadeCoroutine =
                StartCoroutine(Coroutines.FadeTo(_canvasGroup, target, BeatInputService.HALF_NOTE_TIME));
        }

        private void BeatLost() {
            _beatInputService.NoteHit += NoteHit;
            //TriggerFade(0);
        }

        private void NoteHit(NoteQuality quality, float arg2, int arg3) {
            if (quality != NoteQuality.Miss) {
                ResetBeat();
                TriggerFade(1);
                _beatInputService.NoteHit -= NoteHit;
            }
        }

        private void ResetBeat() {
            _metronomeTick = 2;
        }

        private void ExecutionFinished(Song obj) {
            TriggerFade(1);
        }

        private void ExecutionStarted(Song song) {
            TriggerFade(.25f);
        }

        private void Update() {
            _update();
        }
        private void OnDestroy() {
            _beatInputService.ExecutionStarted -= ExecutionStarted;
            _beatInputService.ExecutionFinished -= ExecutionFinished;
            _beatInputService.ExecutionAborted -= ExecutionFinished;
            _gameStateService.GameStarted -= OnGameStarted;
            _beatInputService.NoteHit -= NoteHit;
            _beatInputService.NoteHit -= PunchClosestIndicator;
            _beatInputService.BeatLost -= BeatLost;
        }
    }
}