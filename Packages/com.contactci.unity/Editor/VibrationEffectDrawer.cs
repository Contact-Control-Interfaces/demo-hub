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
        public bool showModifiers = false;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height;
            SerializedProperty takesOptionsProp = property.FindPropertyRelative("TakesOptions");
            if (takesOptionsProp != null && takesOptionsProp.boolValue)
            {
                var options = property.FindPropertyRelative("options");
                var oHeight = EditorGUI.GetPropertyHeight(options, true);
                height = oHeight + EditorGUIUtility.singleLineHeight * EditorGUIUtility.standardVerticalSpacing;
            }
            else
                height = EditorGUIUtility.singleLineHeight * 2;

            if (showModifiers)
                height += EditorGUIUtility.singleLineHeight * 2;
            
            return height;
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
                
                var mRect = new Rect(tRect.x, tRect.y + tRect.height, position.width, tRect.height);
                showModifiers = EditorGUI.Foldout(mRect, showModifiers, "Modifiers");
                if (showModifiers)
                {
                    mRect.x += EditorGUIUtility.standardVerticalSpacing * 4;
                    mRect.height = EditorGUIUtility.singleLineHeight;
                    mRect.width -= EditorGUIUtility.standardVerticalSpacing * 4;
                    mRect.y += mRect.height;
                    EditorGUI.PropertyField(mRect, property.FindPropertyRelative("OneShot"));
                    var delProp = property.FindPropertyRelative("RepeatDelay");
                    mRect.y += mRect.height;
                    EditorGUI.PropertyField(mRect, delProp);
                    var delVal = delProp.intValue;
                    if (delVal % 10 > 0)
                    {
                        delVal -= delVal % 10;
                        delProp.intValue = delVal;
                    }
                }

                var oRect = new Rect(mRect.x, mRect.y + mRect.height, position.width, position.height - mRect.height);
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
