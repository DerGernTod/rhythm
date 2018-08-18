using Services;
using UnityEngine;
using Utils;

namespace Managers {
    public class DrumsManager : MonoBehaviour {
        [SerializeField] private AudioClip _missClip;
        [SerializeField] private AudioClip _badClip;
        [SerializeField] private AudioClip _goodClip;
        [SerializeField] private AudioClip _perfectClip;
        [SerializeField] private AudioClip _startClip;
        [SerializeField] private QualityClipDictionary _clips;
        private BeatInputService _beatInputService;
        private void Start() {
            _clips = new QualityClipDictionary() {
                {BeatQuality.Bad, _badClip},
                {BeatQuality.Good, _goodClip},
                {BeatQuality.Miss, _missClip},
                {BeatQuality.Perfect, _perfectClip},
                {BeatQuality.Start, _startClip}
            };
            _beatInputService = ServiceLocator.Get<BeatInputService>();
            _beatInputService.OnBeatHit += OnBeatHit;
            _beatInputService.OnBeatLost += OnBeatLost;
        }

        private void OnDestroy() {
            _beatInputService.OnBeatHit -= OnBeatHit;
            _beatInputService.OnBeatLost -= OnBeatLost;
        }

        private void OnBeatLost() {
            ServiceLocator.Get<AudioService>().PlayOneShot(_clips[BeatQuality.Miss]);
        }
        
        private void OnBeatHit(BeatQuality quality, float diff, int streak) {
            ServiceLocator.Get<AudioService>().PlayOneShot(_clips[quality]);
        }
    }
}