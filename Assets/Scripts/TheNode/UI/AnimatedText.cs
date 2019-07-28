using System;
using System.Collections.Generic;
using System.Linq;
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
        private string _cleanedText;
        private List<Text> _createdTexts;
        private List<RectTransform> _createdTransforms;
        private List<float> _initialYPositions;
        private float _highlightIndex = -1;

        private void Start() {
            Reinitialize();
        }

        private void OnEnable() {
            Reinitialize();
            #if UNITY_EDITOR
            EditorApplication.update += Update;
            #endif
        }

#if UNITY_EDITOR
        private void OnDisable() {
            // ReSharper disable once DelegateSubtraction
            EditorApplication.update -= Update;
        }
#endif

        public void Reinitialize() {
            _createdTexts = new List<Text>(GetComponentsInChildren<Text>());
            _createdTransforms = new List<RectTransform>();
            _initialYPositions = new List<float>();
            _text = GetComponent<Text>();
            int indexOfSelf = -1;
            TextAnchor alignment = _text.alignment;
            bool doRightToLeft = alignment == TextAnchor.UpperRight
                                 || alignment == TextAnchor.MiddleRight
                                 || alignment == TextAnchor.LowerRight;
            for (int index = 0; index < _createdTexts.Count; index++) {
                int i = index;
                if (doRightToLeft) {
                    i = _createdTexts.Count - index - 1;
                }
                Text createdText = _createdTexts[i];
                if (createdText.gameObject == gameObject) {
                    indexOfSelf = i;
                    continue;
                }

                RectTransform rectTransform = createdText.GetComponent<RectTransform>();
                _createdTransforms.Add(rectTransform);
                _initialYPositions.Add(rectTransform.anchoredPosition.y);
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
            List<float> newYPositions = new List<float>();
            float xPos = 0;
            TextAnchor alignment = _text.alignment;
            bool doRightToLeft = alignment == TextAnchor.UpperRight
                     || alignment == TextAnchor.MiddleRight
                     || alignment == TextAnchor.LowerRight;
            bool doCenter = alignment == TextAnchor.LowerCenter
                            || alignment == TextAnchor.MiddleCenter
                            || alignment == TextAnchor.UpperCenter;
            
            bool doFromUpper = alignment == TextAnchor.UpperCenter
                               || alignment == TextAnchor.UpperLeft
                               || alignment == TextAnchor.UpperRight;
            bool doFromMiddle = alignment == TextAnchor.MiddleCenter
                                || alignment == TextAnchor.MiddleLeft
                                || alignment == TextAnchor.MiddleRight;
            bool doFromLower = alignment == TextAnchor.LowerCenter
                               || alignment == TextAnchor.LowerLeft
                               || alignment == TextAnchor.LowerRight;
            int maxYPos = 0;
            int minYPos = 0;
            List<CharacterInfo> charInfos = new List<CharacterInfo>();
            string originalText = _text.text;
            for (int i = 0; i < originalText.Length; i++) {
                char c = originalText[i];
                if (doRightToLeft) {
                    c = originalText[originalText.Length - 1 - i];
                }
                CharacterInfo charInfo;
                _text.font.GetCharacterInfo(c, out charInfo, _text.fontSize);
                charInfos.Add(charInfo);
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
                textCmp.alignment = _text.alignment;
                textCmp.verticalOverflow = VerticalWrapMode.Overflow;
                textCmp.horizontalOverflow = HorizontalWrapMode.Overflow;
                textCmp.alignByGeometry = true;
                go.transform.SetParent(transform);
                RectTransform goTransform = go.GetComponent<RectTransform>();

                RectTransform.Edge edgeH = RectTransform.Edge.Left;
                if (doRightToLeft) {
                    edgeH = RectTransform.Edge.Right;
                }

                float targetXPos = xPos;
                goTransform.SetInsetAndSizeFromParentEdge(edgeH, targetXPos, charInfo.glyphWidth);
                goTransform.localScale = Vector3.one;
                goTransform.localRotation = Quaternion.identity;
                textCmp.text = newText;
                newTexts.Add(textCmp);
                newTransforms.Add(goTransform);
                maxYPos = Mathf.Max(maxYPos, charInfo.maxY);
                minYPos = Mathf.Min(minYPos, charInfo.minY);
                xPos += advance;
            }

            RectTransform rt = GetComponent<RectTransform>();
            for (int i = 0; i < newTransforms.Count; i++) {
                RectTransform t = newTransforms[i];

                CharacterInfo charInfo = charInfos[i];
                if (doFromUpper) {
                    t.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, maxYPos - charInfo.maxY, charInfo.glyphHeight);    
                } else if (doFromLower) {
                    t.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, charInfo.minY - minYPos, charInfo.glyphHeight);
                } else {
                    t.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, charInfo.minY + rt.rect.height * .5f - maxYPos * .5f - minYPos * .5f, charInfo.glyphHeight);
                }

                Vector2 tAnchoredPosition = t.anchoredPosition;
                if (doCenter) {
                    t.anchoredPosition = new Vector2(tAnchoredPosition.x + rt.rect.width * .5f, tAnchoredPosition.y);
                }
                newYPositions.Add(tAnchoredPosition.y);
            }
            if (doRightToLeft) {
                newTexts.Reverse();
                newTransforms.Reverse();
                newYPositions.Reverse();
            } else if (doCenter) {
                foreach (RectTransform rectTransform in newTransforms) {
                    Vector2 anchoredPosition = rectTransform.anchoredPosition;
                    rectTransform.anchoredPosition = new Vector2(anchoredPosition.x - xPos / 2f, anchoredPosition.y);
                }
            }

            for (int i = originalText.Length; i < _createdTexts.Count; i++) {
                DestroyImmediate(_createdTexts[i].gameObject);
            }

            _createdTransforms = newTransforms;
            _createdTexts = newTexts;
            _initialYPositions = newYPositions;
            _text.text = "";
        }

        public void TriggerImpulse() {
            _highlightIndex = 0;
        }

        public void SetGradient(Gradient newGradient) {
            gradient = newGradient;
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
                    createdText.anchoredPosition = new Vector2(createdText.anchoredPosition.x, _initialYPositions[i] + swingY);
                } else {
                    createdText.anchoredPosition = new Vector2(createdText.anchoredPosition.x, _initialYPositions[i]);
                }
                if (animateColor) {
                    Text text = _createdTexts[i];
                    Color rainbowColor = gradient.Evaluate((1 + Mathf.Sin(Time.time * animationSpeedColor + i * Mathf.PI / _createdTexts.Count)) / 2f);
                    
                    text.color = Color.Lerp(triggerColor, rainbowColor, colorAnimationCurve.Evaluate( Mathf.Min(distance / _createdTexts.Count, 1)));
                }

                createdText.localScale = Vector2.one + Mathf.Min(1 / distance, 2) * Vector2.one;
            }

            _highlightIndex += Mathf.Min(Time.deltaTime * animationSpeedImpulse * 5, _createdTexts.Count * 2);
        }
    }
}