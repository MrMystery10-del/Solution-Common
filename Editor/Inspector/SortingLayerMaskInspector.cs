using Solution.Common.UnityExtension;

using System.Linq;

using UnityEditor;

using UnityEngine;

namespace Solution.Common.Editor.Inspector {

    /** 
    * <summary>
    * Apply to an <b>int</b> field to get a multi-select mask for all Sorting Layers.
    * </summary>
    */
    [CustomPropertyDrawer(typeof(SortingLayerMask))]
    public class SortingLayerMaskDrawer : PropertyDrawer {
    
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            SerializedProperty maskProperty = property.FindPropertyRelative("mask");

            var layerNames = SortingLayer.layers.Select(layer => layer.name).ToArray();
            int currentMask = maskProperty.intValue;
            int newMask = EditorGUI.MaskField(position, label, currentMask, layerNames);

            if (newMask != currentMask) {
                maskProperty.intValue = newMask;
            }
        }
    }
}