using Rhythm.Services;
using Rhythm.Utils;
using UnityEngine;

namespace Rhythm.Managers {
    public class DrumsManager : MonoBehaviour {
#pragma warning disable 0649
        [SerializeField] private AudioClip missClip;
        [SerializeField] private AudioClip badClip;
        [SerializeField] private AudioClip goodClip;
        [SerializeField] private AudioClip perfectClip;
        [SerializeField] private AudioClip startClip;
#pragma warning restore 0649
        private QualityClipDictionary clips;
        private BeatInputService _beatInputService;
        private void Start() {
            clips = new QualityClipDictionary {
                {NoteQuality.Bad, badClip},
                {NoteQuality.Good, goodClip},
                {NoteQuality.Miss, missClip},
                {NoteQuality.Perfect, perfectClip}
            };
            _beatInputService = ServiceLocator.Get<BeatInputService>();
            _beatInputService.OnNoteHit += OnNoteHit;
            _beatInputService.OnBeatLost += OnBeatLost;
        }

        private void OnDestroy() {
            _beatInputService.OnNoteHit -= OnNoteHit;
            _beatInputService.OnBeatLost -= OnBeatLost;
        }

        private void OnBeatLost() {
            ServiceLocator.Get<AudioService>().PlayOneShot(clips[NoteQuality.Miss]);
        }
        
        private void OnNoteHit(NoteQuality quality, float diff, int streak) {
            ServiceLocator.Get<AudioService>().PlayOneShot(clips[quality]);
        }
    }
}