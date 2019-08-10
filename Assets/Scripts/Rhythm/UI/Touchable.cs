using System.Collections;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Rhythm.UI {
    public class Touchable : MonoBehaviour {

#pragma warning disable 0649
        [SerializeField] private Color touchColor = Color.white;
        [SerializeField] private UnityEvent onTouch;
#pragma warning restore 0649
        
        private Color _initTint;
        private static readonly int TintProp = Shader.PropertyToID("_Tint");
        private bool _touchEnabled = true;
        private Material _mat;

        private void Start() {
            _mat = GetComponent<Renderer>().material;
            _initTint = _mat.GetColor(TintProp);
            gameObject.layer = LayerMask.NameToLayer(Constants.LAYER_TOUCHABLES);
        }

        private void OnMouseDown() {
            if (_touchEnabled) {
                onTouch?.Invoke();
                _touchEnabled = false;
                StartCoroutine(LerpHighlightColor(touchColor,_initTint, .5f));
            }
        }

        private IEnumerator LerpHighlightColor(Color from, Color to, float time) {
            float curTime = 0;
            while (curTime < time) {
                _mat.SetColor(TintProp, Color.Lerp(from, to, curTime / time));
                curTime += Time.deltaTime;
                yield return null;
            }
            _mat.SetColor(TintProp, to);
            _touchEnabled = true;
        }
    }
}