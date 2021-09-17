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