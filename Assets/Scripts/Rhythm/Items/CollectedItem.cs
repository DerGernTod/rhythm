using UnityEngine;
using Rhythm.Utils;

namespace Rhythm.Items {
    [RequireComponent(typeof(SpriteRenderer))]
    public class CollectedItem : MonoBehaviour {
        private SpriteRenderer _spriteRenderer;
        private void Awake() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(Sprite sprite, AnimationCurve itemSpawnYCurve) {
            _spriteRenderer.sprite = sprite;

            Color prevColor = _spriteRenderer.color;
            Color transparent = new Color(prevColor.r, prevColor.g, prevColor.b, 0);
            _spriteRenderer.color = transparent;

            StartCoroutine(Coroutines.FadeColor(gameObject, prevColor, 1));
            StartCoroutine(Coroutines.MoveAlongCurve(transform, itemSpawnYCurve, Vector3.up, 1f, false,
                () => {
                    StartCoroutine(Coroutines.FadeColor(gameObject, transparent, 2, () => {
                        Destroy(gameObject);
                    }));

                }));
        }
    }
}