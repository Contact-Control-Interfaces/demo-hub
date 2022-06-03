using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Maestro
{
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
}
