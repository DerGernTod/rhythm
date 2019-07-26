using UnityEditor;
using UnityEngine;

namespace TheNode.UI.Editor {
    [CustomEditor(typeof(AnimatedText))]
    public class AnimatedTextEditor: UnityEditor.Editor {
        public override void OnInspectorGUI() {
            
            EditorGUILayout.BeginVertical();
            base.OnInspectorGUI();
            AnimatedText myTarget = (AnimatedText) target;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Trigger Impulse")) {
                myTarget.TriggerImpulse();
            }
            if (GUILayout.Button("Regenerate")) {
                myTarget.Refresh();
            }
            if (GUILayout.Button("Reinitialize")) {
                myTarget.Reinitialize();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }
}