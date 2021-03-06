﻿using Rhythm.Services;
using Rhythm.Utils;
using UnityEngine;

namespace Rhythm.Managers {
    public class DrumsManager : MonoBehaviour {
#pragma warning disable 0649
        [SerializeField] private AudioClip missClip;
        [SerializeField] private AudioClip badClip;
        [SerializeField] private AudioClip goodClip;
        [SerializeField] private AudioClip perfectClip;
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
            _beatInputService.NoteHit += NoteHit;
            _beatInputService.BeatLost += BeatLost;
        }

        private void OnDestroy() {
            _beatInputService.NoteHit -= NoteHit;
            _beatInputService.BeatLost -= BeatLost;
        }

        private void BeatLost() {
            ServiceLocator.Get<AudioService>().PlayOneShot(clips[NoteQuality.Miss]);
        }
        
        private void NoteHit(NoteQuality quality, float diff) {
            ServiceLocator.Get<AudioService>().PlayOneShot(clips[quality]);
        }
    }
}