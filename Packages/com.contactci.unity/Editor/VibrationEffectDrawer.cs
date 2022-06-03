using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Maestro.Vibration;

namespace Maestro
{
    [CustomPropertyDrawer(typeof(VibrationEffect))]
    public class VibrationEffectDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.serializedObject.Update();

            EditorGUI.BeginProperty(position, label, property);

            // Select type
            SerializedProperty whichType = property.FindPropertyRelative("WhichType");
            if (whichType != null) {
                SerializedProperty takesOptionsProp = property.FindPropertyRelative("TakesOptions");
                SerializedProperty options = property.FindPropertyRelative("options");

                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = whichType.hasMultipleDifferentValues;

                EditorGUILayout.PropertyField(whichType, new GUIContent("Vibration Effect"));

                if (takesOptionsProp.boolValue && !whichType.hasMultipleDifferentValues)
                    EditorGUILayout.PropertyField(options, new GUIContent("Vibration Options"), true);
                else if (whichType.hasMultipleDifferentValues)
                    EditorGUILayout.HelpBox("Multiple effect type values detected! Options not available.", MessageType.Warning, wide: true);

            } else {
                EditorGUILayout.HelpBox("No type found!", MessageType.Warning, wide: true);
            }
            EditorGUI.showMixedValue = false;
            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
