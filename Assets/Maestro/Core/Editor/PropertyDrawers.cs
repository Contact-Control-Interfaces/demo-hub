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
            property.serializedObject.Update();

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
                EditorGUILayout.PropertyField(vibrationProp, GUIContent.none, true);
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

                EffectType type;

                // Set this effect to None if it isn't anything already
                if (!GetValue(whichType, out type)) {
                    type = EffectType.None;
                    takesOptionsProp.boolValue = false;
                    options.managedReferenceValue = null;
                } else {
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.showMixedValue = whichType.hasMultipleDifferentValues;

                    EditorGUILayout.PropertyField(whichType, new GUIContent("Vibration Effect"));
                    if (whichType.hasMultipleDifferentValues)
                        EditorGUILayout.HelpBox("Multiple effect type values detected! Options not available.", MessageType.Warning, wide: true);

                    // Primary effect type changed, change options to match
                    if (EditorGUI.EndChangeCheck()) {

                        EffectType newType;
                        if (GetValue(whichType, whichType.intValue, out newType)) {
                            type = newType;
                            UpdateOptions(options, takesOptionsProp, type);
                            property.serializedObject.ApplyModifiedProperties();
                        } else {
                            Debug.LogError("Could not parse VibrationEffect type!");
                        }
                    } else if (options != null && (EffectType)whichType.intValue != EffectType.None) {
                        SerializedProperty optionsType = options.FindPropertyRelative("Type");

                        // Options effect type changed, undo the change
                        if (optionsType != null && whichType != null && ((optionsType.enumValueIndex != whichType.enumValueIndex)
                            || (optionsType.hasMultipleDifferentValues && !whichType.hasMultipleDifferentValues))) {
                            UpdateOptions(options, takesOptionsProp, type);
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    }

                    if (takesOptionsProp.boolValue && !whichType.hasMultipleDifferentValues) {
                        DisplayOptions(options, type);
                    }
                }
            } else {
                EditorGUILayout.LabelField("no type found!");
            }
            EditorGUI.showMixedValue = false;
            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
        }

        private void UpdateOptions(SerializedProperty optionsProp, SerializedProperty takesOptionsProp, EffectType type)
        {
            EffectOptions newOptions = EffectOptions.ConstructFromType(type);
            takesOptionsProp.boolValue = newOptions != null;
            optionsProp.managedReferenceValue = newOptions;

            foreach (object obj in optionsProp.serializedObject.targetObjects)
            {
                // This is gross but it works
                MaestroInteractable interactable = obj as MaestroInteractable;
                if (interactable != null) {
                    VibrationEffect vibration = interactable.haptics.Vibration;
                    vibration.options = EffectOptions.ConstructFromType(type);
                    continue;
                }

                MaestroHand hand = obj as MaestroHand;
                if (hand != null) {
                    VibrationEffect vibration = hand.defaultEffectOverride.Vibration;
                    vibration.options = EffectOptions.ConstructFromType(type);
                }
            }
        }

        private void DisplayOptions(SerializedProperty prop, EffectType type)
        {
            prop.serializedObject.Update();

            if (prop == null) {
                EditorGUILayout.LabelField("no options found!");
                return;
            }

            SerializedProperty typeProp = prop.FindPropertyRelative("Type");
            if (typeProp == null) {
                prop.managedReferenceValue = EffectOptions.ConstructFromType(type);
            }
            
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            EditorGUILayout.PropertyField(prop, new GUIContent("Effect Config"), true);

            prop.serializedObject.ApplyModifiedProperties();
        }

        private bool GetValue<T>(SerializedProperty prop, out T value) where T : struct, Enum
        {
            return GetValue(prop, prop.enumValueIndex, out value);
        }

        private bool GetValue<T>(SerializedProperty prop, int index, out T value) where T : struct, Enum
        {
            if (index < 0 || index >= prop.enumNames.Length) {
                value = default(T); 
                return false;
            }
            bool success = Enum.TryParse<T>(prop.enumNames[index], out T result);
            value = result;
            return success;
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