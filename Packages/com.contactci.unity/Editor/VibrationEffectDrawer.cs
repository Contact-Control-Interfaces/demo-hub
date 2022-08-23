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
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty takesOptionsProp = property.FindPropertyRelative("TakesOptions");
            if (takesOptionsProp != null && takesOptionsProp.boolValue)
            {
                var options = property.FindPropertyRelative("options");
                var oHeight = EditorGUI.GetPropertyHeight(options, true);
                return oHeight + EditorGUIUtility.singleLineHeight;
            }

            return EditorGUIUtility.singleLineHeight;
        }

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

                var tRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
                EditorGUI.PropertyField(tRect, whichType, new GUIContent("Vibration Effect"));

                var oRect = new Rect(tRect.x, tRect.y + tRect.height, position.width, position.height - tRect.height);
                if (takesOptionsProp.boolValue && !whichType.hasMultipleDifferentValues)
                    EditorGUI.PropertyField(oRect, options, new GUIContent("Vibration Options"), true);
                else if (whichType.hasMultipleDifferentValues)
                    EditorGUI.HelpBox(oRect, "Multiple effect type values detected! Options not available.", MessageType.Warning);

            } else {
                EditorGUI.HelpBox(position, "No type found!", MessageType.Warning);
            }
            EditorGUI.showMixedValue = false;
            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
