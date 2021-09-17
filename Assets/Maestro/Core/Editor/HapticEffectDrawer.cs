using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Maestro
{
    [CustomPropertyDrawer(typeof(HapticEffect))]
    public class HapticEffectDrawer : PropertyDrawer
    {
        private bool hasLabel = false;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => hasLabel ? EditorGUIUtility.singleLineHeight : 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            if (hasLabel)
                EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            SerializedProperty amplitudeProp = property.FindPropertyRelative("Amplitude");
            EditorGUI.showMixedValue = amplitudeProp.hasMultipleDifferentValues;
            EditorGUILayout.IntSlider(
                amplitudeProp,
                HapticEffect.FORCE_FEEDBACK_MIN_AMPLITUDE,
                HapticEffect.FORCE_FEEDBACK_MAX_AMPLITUDE);

            amplitudeProp.serializedObject.ApplyModifiedProperties();

            SerializedProperty vibrationProp = property.FindPropertyRelative("Vibration");
            if (vibrationProp != null) {
                EditorGUILayout.PropertyField(vibrationProp, new GUIContent("Vibration Effect"), true);
            } else {
                EditorGUILayout.HelpBox("Vibration effect not found!", MessageType.Error);
            }
            vibrationProp.serializedObject.ApplyModifiedProperties();

            EditorGUI.EndProperty();

            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.showMixedValue = false;
        }
    }
}