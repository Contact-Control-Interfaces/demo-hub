using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Maestro
{
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
            // Thumb doesn't have distal joint
            //updateObjectField(property, "ThumbDistal");
            updateObjectField(property, "ThumbMiddle");
            updateObjectField(property, "ThumbKnuckle");
            EditorGUILayout.Space();
            updateObjectField(property, "IndexTip");
            updateObjectField(property, "IndexDistal");
            updateObjectField(property, "IndexMiddle");
            updateObjectField(property, "IndexKnuckle");
            EditorGUILayout.Space();
            updateObjectField(property, "MiddleTip");
            updateObjectField(property, "MiddleDistal");
            updateObjectField(property, "MiddleMiddle");
            updateObjectField(property, "MiddleKnuckle");
            EditorGUILayout.Space();
            updateObjectField(property, "RingTip");
            updateObjectField(property, "RingDistal");
            updateObjectField(property, "RingMiddle");
            updateObjectField(property, "RingKnuckle");
            EditorGUILayout.Space();
            updateObjectField(property, "LittleTip");
            updateObjectField(property, "LittleDistal");
            updateObjectField(property, "LittleMiddle");
            updateObjectField(property, "LittleKnuckle");
            EditorGUILayout.Space();
            updateObjectField(property, "PalmBaseThumb");
            updateObjectField(property, "PalmBaseLittle");

            updateObjectField(property, "BetweenIndexMiddle");
            updateObjectField(property, "BetweenRingLittle");
            updateObjectField(property, "PalmCreaseThumb");
            updateObjectField(property, "PalmCreaseMiddle");
            updateObjectField(property, "PalmCreaseLittle");

            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}