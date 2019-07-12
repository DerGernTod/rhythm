using Rhythm.Services;
using Rhythm.Utils;
using UnityEngine;

namespace Rhythm.Managers {
    public class DrumsManager : MonoBehaviour {
        [SerializeField] private AudioClip missClip;
        [SerializeField] private AudioClip badClip;
        [SerializeField] private AudioClip goodClip;
        [SerializeField] private AudioClip perfectClip;
        [SerializeField] private AudioClip startClip;
        private QualityClipDictionary clips;
        private BeatInputService _beatInputService;
        private void Start() {
            clips = new QualityClipDictionary {
                {BeatQuality.Bad, badClip},
                {BeatQuality.Good, goodClip},
                {BeatQuality.Miss, missClip},
                {BeatQuality.Perfect, perfectClip},
                {BeatQuality.Start, startClip}
            };
            _beatInputService = ServiceLocator.Get<BeatInputService>();
            _beatInputService.BeatHit += BeatHit;
            _beatInputService.OnBeatLost += OnBeatLost;
        }

        private void OnDestroy() {
            _beatInputService.BeatHit -= BeatHit;
            _beatInputService.OnBeatLost -= OnBeatLost;
        }

        private void OnBeatLost() {
            ServiceLocator.Get<AudioService>().PlayOneShot(clips[BeatQuality.Miss]);
        }
        
        private void BeatHit(BeatQuality quality, float diff, int streak) {
            ServiceLocator.Get<AudioService>().PlayOneShot(clips[quality]);
        }
    }
}