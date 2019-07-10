using UnityEngine;

namespace Rhythm.Tools {
    [CreateAssetMenu(fileName = "Data", menuName = "Rhythm/Tools/ToolTypeData", order = 1)]
    public class ToolTypeData: ScriptableObject {
        public string toolTypeName;
        public string toolTypeDescription;
        public Sprite sprite;
    }
}