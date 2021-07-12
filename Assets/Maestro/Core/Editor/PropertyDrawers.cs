using UnityEditor;
using UnityEngine;
using Maestro.Vibration;
using System;

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

    [CustomPropertyDrawer(typeof(HandSize))]
    public class HandSizeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.serializedObject.Update();

            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            property.FindPropertyRelative("TipSize").floatValue = EditorGUILayout.FloatField("Tip Size", property.FindPropertyRelative("TipSize").floatValue);
            property.FindPropertyRelative("MiddleSize").floatValue = EditorGUILayout.FloatField("Middle Size", property.FindPropertyRelative("MiddleSize").floatValue);
            property.FindPropertyRelative("KnuckleSize").floatValue = EditorGUILayout.FloatField("Knuckle Size", property.FindPropertyRelative("KnuckleSize").floatValue);

            EditorGUI.EndProperty();

            property.serializedObject.ApplyModifiedProperties();
        }
    }

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

    [CustomPropertyDrawer(typeof(HandTransforms))]
    public class HandTransformsDrawer : PropertyDrawer
    {
        private void updateObjectField(SerializedProperty property, string propertyName)
        {
            property.FindPropertyRelative(propertyName).objectReferenceValue = 
                (Transform)EditorGUILayout.ObjectField(ObjectNames.NicifyVariableName(propertyName), 
                property.FindPropertyRelative(propertyName).objectReferenceValue, 
                typeof(Transform), allowSceneObjects: true);

        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.serializedObject.Update();

            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            updateObjectField(property, "ThumbTip");
            updateObjectField(property, "ThumbMiddle");
            updateObjectField(property, "ThumbKnuckle");
            EditorGUILayout.Space();
            updateObjectField(property, "IndexTip");
            updateObjectField(property, "IndexMiddle");
            updateObjectField(property, "IndexKnuckle");
            EditorGUILayout.Space();
            updateObjectField(property, "MiddleTip");
            updateObjectField(property, "MiddleMiddle");
            updateObjectField(property, "MiddleKnuckle");
            EditorGUILayout.Space();
            updateObjectField(property, "RingTip");
            updateObjectField(property, "RingMiddle");
            updateObjectField(property, "RingKnuckle");
            EditorGUILayout.Space();
            updateObjectField(property, "LittleTip");
            updateObjectField(property, "LittleMiddle");
            updateObjectField(property, "LittleKnuckle"); 
            EditorGUILayout.Space();
            updateObjectField(property, "PalmBase");

            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}