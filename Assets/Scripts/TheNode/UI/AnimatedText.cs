using System;
using System.Collections.Generic;
using System.Linq;
using Rhythm.Services;
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

        public void StartAnimation(float swingSpeed = 1 / BeatInputService.NOTE_TIME) {
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
            List<float> xOffsets = new List<float>();
            string originalText = _text.text;
            int actualFontSize = _text.cachedTextGenerator.fontSizeUsedForBestFit;
            for (int i = 0; i < originalText.Length; i++) {
                char c = originalText[i];
                if (doRightToLeft) {
                    c = originalText[originalText.Length - 1 - i];
                }
                CharacterInfo charInfo;
                _text.font.GetCharacterInfo(c, out charInfo, actualFontSize);
                charInfos.Add(charInfo);
                string newText = "" + c;
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
                textCmp.fontSize = actualFontSize;
                textCmp.color = _text.color;
                textCmp.alignment = _text.alignment;
                textCmp.verticalOverflow = VerticalWrapMode.Truncate;
                textCmp.horizontalOverflow = HorizontalWrapMode.Wrap;
                textCmp.alignByGeometry = _text.alignByGeometry;
                go.transform.SetParent(transform);
                RectTransform goTransform = go.GetComponent<RectTransform>();
                goTransform.localScale = Vector3.one;
                goTransform.localRotation = Quaternion.identity;
                goTransform.anchoredPosition = Vector2.zero;
                goTransform.anchorMin = Vector2.zero;
                goTransform.anchorMax = Vector2.one;
                textCmp.text = newText;
                newTexts.Add(textCmp);
                newTransforms.Add(goTransform);
                maxYPos = Mathf.Max(maxYPos, charInfo.maxY);
                minYPos = Mathf.Min(minYPos, charInfo.minY);
                xPos += charInfo.advance;
                xOffsets.Add(xPos);
            }

            RectTransform rt = GetComponent<RectTransform>();
            Rect parentRect = rt.rect;
            int smallestFontSize = 999;
            for (int i = 0; i < newTransforms.Count; i++) {
                RectTransform t = newTransforms[i];
                CharacterInfo charInfo = charInfos[i];
                Vector2 glyphSize = new Vector2(
                    charInfo.maxX - charInfo.minX,
                    charInfo.maxY - charInfo.minY);
                Vector2 pivot = Vector2.zero;
                Vector2 targetPos = Vector2.zero;
                RectTransform.Edge verticalEdge = RectTransform.Edge.Bottom;
                RectTransform.Edge horizontalEdge = RectTransform.Edge.Left;
                
                // vertical
                if (doFromUpper) {
                    pivot.y = 1;
                    targetPos.y = -charInfo.maxY - minYPos;
                    verticalEdge = RectTransform.Edge.Top;
                } else if (doFromLower) {
                    targetPos.y = charInfo.maxY;
                } else {
                    pivot.y = .5f;
                    targetPos.y = charInfo.minY + glyphSize.y * .5f;
                }
                
                //horizontal
                float curOffset = xOffsets[i];
                if (doCenter) {
                    pivot.x = .5f;
                    targetPos.x = curOffset + (parentRect.width - xPos - glyphSize.x) * .5f;
                } else if (doRightToLeft) {
                    pivot.x = 1f;
                    horizontalEdge = RectTransform.Edge.Right;
                    targetPos.x = curOffset - parentRect.width;
                } else {
                    targetPos.x = curOffset;
                }

                t.pivot = pivot;
                t.SetInsetAndSizeFromParentEdge(horizontalEdge, targetPos.x, glyphSize.x);
                t.SetInsetAndSizeFromParentEdge(verticalEdge, targetPos.y, glyphSize.y);

                Vector2 anchoredPosition = t.anchoredPosition;
                Vector2 rectSize = new Vector2(parentRect.width, parentRect.height);
                t.anchorMax = (anchoredPosition + glyphSize) / rectSize;
                t.anchorMin = (anchoredPosition - glyphSize) / rectSize;
                t.offsetMax = Vector2.zero;
                t.offsetMin = Vector2.zero;
                newYPositions.Add(t.anchoredPosition.y);
                // TODO: uncomment this when size problem is resolved
                Text textCmp = newTexts[i];
                textCmp.resizeTextForBestFit = _text.resizeTextForBestFit;
                textCmp.resizeTextMaxSize = _text.resizeTextMaxSize;
                textCmp.resizeTextMinSize = _text.resizeTextMinSize;
            }
            if (doRightToLeft) {
                newTexts.Reverse();
                newTransforms.Reverse();
                newYPositions.Reverse();
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

                createdText.localScale = Vector3.one + Mathf.Min(1 / distance, 2) * Vector3.one;
            }

            _highlightIndex += Mathf.Min(Time.deltaTime * animationSpeedImpulse * 5, _createdTexts.Count * 2);
        }
    }
}