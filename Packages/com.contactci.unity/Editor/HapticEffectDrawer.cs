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

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Vibration")) +
                   EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            if (hasLabel)
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            
            SerializedProperty amplitudeProp = property.FindPropertyRelative("Amplitude");
            var pHeight = EditorGUI.GetPropertyHeight(amplitudeProp);
            EditorGUI.showMixedValue = amplitudeProp.hasMultipleDifferentValues;
            var sRect = new Rect(position.x, position.y, position.width, pHeight);
            EditorGUI.IntSlider(
                sRect,
                amplitudeProp,
                HapticEffect.FORCE_FEEDBACK_MIN_AMPLITUDE,
                HapticEffect.FORCE_FEEDBACK_MAX_AMPLITUDE);

            amplitudeProp.serializedObject.ApplyModifiedProperties();
            SerializedProperty vibrationProp = property.FindPropertyRelative("Vibration");
            
            if (vibrationProp != null)
            {
                pHeight += EditorGUIUtility.standardVerticalSpacing;
                var vHeight = EditorGUI.GetPropertyHeight(vibrationProp);
                var pRect = new Rect(position.x, position.y + pHeight, position.width, vHeight);
                EditorGUI.PropertyField(pRect, vibrationProp, new GUIContent("Vibration Effect"), true);
                vibrationProp.serializedObject.ApplyModifiedProperties();
            } else {
                EditorGUI.HelpBox(position, "Vibration effect not found!", MessageType.Error);
            }

            EditorGUI.EndProperty();

            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.showMixedValue = false;
        }
    }
}