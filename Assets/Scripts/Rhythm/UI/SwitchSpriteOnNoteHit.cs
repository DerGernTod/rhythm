using System;
using System.Collections;
using Rhythm.Services;
using UnityEngine;

namespace Rhythm.UI {
    [RequireComponent(typeof(SpriteRenderer))]
    public class SwitchSpriteOnNoteHit : MonoBehaviour {
        [SerializeField] private Sprite swapSprite;

        private Sprite _originalSprite;
        private SpriteRenderer _spriteRenderer;
        private void Start() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _originalSprite = _spriteRenderer.sprite;
        }

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                StopAllCoroutines();
                _spriteRenderer.sprite = swapSprite;
                StartCoroutine(SwapBackToOriginal());
            }
        }

        private IEnumerator SwapBackToOriginal() {
            yield return new WaitForSeconds(BeatInputService.FAIL_TOLERANCE);
            _spriteRenderer.sprite = _originalSprite;
        }
    }
}