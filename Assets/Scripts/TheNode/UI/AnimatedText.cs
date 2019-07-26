using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TheNode.UI {
    [RequireComponent(typeof(Text))]
    [ExecuteInEditMode]
    public class AnimatedText : MonoBehaviour {
#pragma warning disable 0649
        [FormerlySerializedAs("animate")] [SerializeField] private bool animateColor;
        [SerializeField] private bool animatePosition;
        [SerializeField] private bool autoGenerateOnChange = false;
        [SerializeField] private Color triggerColor;
        [FormerlySerializedAs("animationSpeed")] [Range(.1f, 10f)] [SerializeField] private float animationSpeedImpulse = 3f;
        [Range(.1f, 10f)] [SerializeField] private float animationSpeedColor = 3f;
        [Range(.1f, 10f)] [SerializeField] private float animationSpeedSwing = 3f;
        [SerializeField] private Gradient gradient;
        [FormerlySerializedAs("curve")] [SerializeField] private AnimationCurve colorAnimationCurve;
        [SerializeField] private AnimationCurve swingAnimationCurve;
#pragma warning restore 0649

        private Text _text;
        private string _originalText;
        private string _cleanedText;
        private List<Text> _createdTexts;
        private List<RectTransform> _createdTransforms;
        private float _highlightIndex = -1;

        private void Start() {
            Reinitialize();
        }

        private void OnEnable() {
            Reinitialize();
            EditorApplication.update += Update;
        }

        private void OnDisable() {
            // ReSharper disable once DelegateSubtraction
            EditorApplication.update -= Update;
        }

        public void Reinitialize() {
            _createdTexts = new List<Text>(GetComponentsInChildren<Text>());
            _createdTransforms = new List<RectTransform>();
            _text = GetComponent<Text>();
            int indexOfSelf = -1;
            for (int index = 0; index < _createdTexts.Count; index++) {
                Text createdText = _createdTexts[index];
                if (createdText.gameObject == gameObject) {
                    indexOfSelf = index;
                    continue;
                }
                RectTransform rectTransform = createdText.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 0);
                _createdTransforms.Add(rectTransform);
            }
            _createdTexts.RemoveAt(indexOfSelf);
        }

        public void StartAnimation(float swingSpeed) {
            animationSpeedSwing = swingSpeed;
            animateColor = true;
            animatePosition = true;
        }

        public void Refresh() {
            List<RectTransform> newTransforms = new List<RectTransform>();
            List<Text> newTexts = new List<Text>();
            _originalText = _text.text;
            float xPos = 0;
            for (int i = 0; i < _originalText.Length; i++) {
                char c = _originalText[i];
                CharacterInfo charInfo;
                _text.font.GetCharacterInfo(_text.text[i], out charInfo, _text.fontSize);
                string newText = "" + c;
                float pixelsPerUnit = _text.pixelsPerUnit;
                float advance = charInfo.advance * pixelsPerUnit;
                bool reuse = i < _createdTexts.Count;
                GameObject go;
                Text textCmp;
                if (reuse) {
                    go = _createdTexts[i].gameObject;
                    go.name = newText;
                    textCmp = go.GetComponent<Text>();
                    go.transform.SetParent(null);
                } else {
                    go = new GameObject(newText);
                    textCmp = go.AddComponent<Text>();
                }
                textCmp.font = _text.font;
                textCmp.fontSize = _text.fontSize;
                textCmp.color = _text.color;
                textCmp.alignment = TextAnchor.LowerLeft;
                textCmp.verticalOverflow = VerticalWrapMode.Overflow;
                textCmp.horizontalOverflow = HorizontalWrapMode.Overflow;
                textCmp.alignByGeometry = true;
                go.transform.SetParent(transform);
                RectTransform goTransform = go.GetComponent<RectTransform>();
                goTransform.sizeDelta = new Vector2(1, 1);
                goTransform.anchorMin = Vector2.zero;
                goTransform.anchorMax = Vector2.zero;
                // goTransform.pivot = Vector2.one * .5f;
                goTransform.anchoredPosition = new Vector2(xPos, 0);
                goTransform.localScale = Vector3.one;
                goTransform.localRotation = Quaternion.identity;
                textCmp.text = newText;
                newTexts.Add(textCmp);
                newTransforms.Add(goTransform);
                xPos += advance;
            }

            for (int i = _originalText.Length; i < _createdTexts.Count; i++) {
                DestroyImmediate(_createdTexts[i].gameObject);
            }

            _createdTransforms = newTransforms;
            _createdTexts = newTexts;
            _text.text = "";
        }

        public void TriggerImpulse() {
            _highlightIndex = 0;
        }

        private void Update() {
            if (_text.text != "" && autoGenerateOnChange) {
                Refresh();
            }
            for (int i = 0; i < _createdTransforms.Count; i++) {
                if (_createdTransforms[i] == null) {
                    continue;
                }
                float distance = Mathf.Abs(i - _highlightIndex);
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (distance == 0) {
                    distance = 0.01f;
                }
                RectTransform createdText = _createdTransforms[i];
                if (animatePosition) {
                    float swingY =
                        4 * swingAnimationCurve.Evaluate(
                            (Time.time * animationSpeedSwing + (_createdTexts.Count - i) * .1f) % 1) - 2;
                    createdText.anchoredPosition =
                        new Vector2(createdText.anchoredPosition.x,
                            swingY);
                }
                if (animateColor) {
                    Text text = _createdTexts[i];
                    Color rainbowColor = gradient.Evaluate((1 + Mathf.Sin(-Time.time * animationSpeedColor + +i * Mathf.PI / _createdTexts.Count)) / 2f);
                    
                    text.color = Color.Lerp(triggerColor, rainbowColor, colorAnimationCurve.Evaluate( Mathf.Min(distance / _createdTexts.Count, 1)));
                }

                createdText.localScale = Vector2.one + Mathf.Min(1 / distance, 2) * Vector2.one;
            }

            _highlightIndex += Mathf.Min(Time.deltaTime * animationSpeedImpulse * 5, _createdTexts.Count * 2);
        }
    }
}