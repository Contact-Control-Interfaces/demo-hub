using UnityEditor;
using UnityEngine;

namespace Maestro
{
    [CustomPropertyDrawer(typeof(HapticEffect))]
    public class HapticEffectDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            property.FindPropertyRelative("Amplitude").intValue =
                (byte)EditorGUILayout.IntSlider("Feedback Amplitude",
                property.FindPropertyRelative("Amplitude").intValue,
                HapticEffect.FORCE_FEEDBACK_MIN_AMPLITUDE,
                HapticEffect.FORCE_FEEDBACK_MAX_AMPLITUDE);
            property.FindPropertyRelative("Vibration").intValue =
                (byte)EditorGUILayout.IntSlider("Vibration Effect",
                property.FindPropertyRelative("Vibration").intValue,
                HapticEffect.VIBRATION_MIN_ID,
                HapticEffect.VIBRATION_MAX_ID);
            EditorGUILayout.HelpBox(DRV2605Descriptions.get(property.FindPropertyRelative("Vibration").intValue), MessageType.None, false);

            EditorGUI.EndProperty();
        }
    }
    
    [CustomPropertyDrawer(typeof(HandSize))]
    public class HandSizeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            property.FindPropertyRelative("TipSize").floatValue = EditorGUILayout.FloatField("Tip Size", property.FindPropertyRelative("TipSize").floatValue);
            property.FindPropertyRelative("MiddleSize").floatValue = EditorGUILayout.FloatField("Middle Size", property.FindPropertyRelative("MiddleSize").floatValue);
            property.FindPropertyRelative("KnuckleSize").floatValue = EditorGUILayout.FloatField("Knuckle Size", property.FindPropertyRelative("KnuckleSize").floatValue);

            EditorGUI.EndProperty();
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
        }
    }
}