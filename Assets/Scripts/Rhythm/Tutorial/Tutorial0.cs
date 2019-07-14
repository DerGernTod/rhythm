using System;
using System.Collections;
using Rhythm.Services;
using Rhythm.UI;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Rhythm.Tutorial {
    public class Tutorial0 : MonoBehaviour {
        
#pragma warning disable 0649
        [SerializeField] private ComicPage pageToScale;
        [SerializeField] private Image drum;
#pragma warning restore 0649
        
        public void OnComicPageCompleted(int page) {
            if (page == 1) {
                ServiceLocator.Get<GameStateService>().TriggerGameStarted();
                iTween.ScaleBy(pageToScale.gameObject, Vector3.one * 1.05f, 5);
                ServiceLocator.Get<BeatInputService>().OnMetronomeTick += PunchDrum;
            }
        }

        private void OnDestroy() {
            ServiceLocator.Get<BeatInputService>().OnMetronomeTick -= PunchDrum;
        }

        private void PunchDrum() {
            iTween.PunchScale(drum.gameObject, Vector3.one * .5f, BeatInputService.HALF_NOTE_TIME);
            iTween.PunchRotation(drum.gameObject, Vector3.forward * .5f, BeatInputService.HALF_NOTE_TIME);
        }

    }
}