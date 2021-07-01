using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Maestro
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MaestroInteractable))]
    public class MaestroInteractableEditor : Editor
    {
        private bool showAdvConfig = false;
        private string showAdvConfigTxt = "Advanced Configuration";

        private bool showEvents = false;
        private string showEventsTxt = "Bind Events";

        private SerializedProperty OnTouchProp, UnTouchProp, OnGrabProp, OnReleaseProp;
        private SerializedProperty haptics;
        private SerializedProperty type, ignoreTaps, persist, persistenceDuration;
        private SerializedProperty UseRenderCenter, maintainOrientation, maintainPosition, stayInHand, SendHapticsToWholeHand;
        private SerializedProperty gripTransform, gripCollider;

        private void OnEnable()
        {
            OnTouchProp = serializedObject.FindProperty("onTouch");
            UnTouchProp = serializedObject.FindProperty("unTouch");
            OnGrabProp = serializedObject.FindProperty("onGrab");
            OnReleaseProp = serializedObject.FindProperty("onRelease");
            haptics = serializedObject.FindProperty("haptics");

            type = serializedObject.FindProperty("type");
            ignoreTaps = serializedObject.FindProperty("IgnoreTaps");
            persist = serializedObject.FindProperty("isPersistent");
            persistenceDuration = serializedObject.FindProperty("persistenceDuration");

            UseRenderCenter = serializedObject.FindProperty("UseRenderCenter");
            maintainOrientation = serializedObject.FindProperty("maintainOrientation");
            maintainPosition = serializedObject.FindProperty("maintainPosition");
            stayInHand = serializedObject.FindProperty("stayInHand");
            SendHapticsToWholeHand = serializedObject.FindProperty("SendHapticsToWholeHand");

            gripTransform = serializedObject.FindProperty("gripTransform");
            gripCollider = serializedObject.FindProperty("gripCollider");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MaestroInteractable mi = (MaestroInteractable)target;

            /** 
             * Haptics
             */
            EditorGUILayout.PropertyField(haptics);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            EditorGUILayout.Space();

            /**
             * Configuration
             */
            PropertyField(type);
            PropertyField(ignoreTaps);
            PropertyField(persist);
            if (persist.boolValue) {
                PropertyField(persistenceDuration);
            }
            EditorGUILayout.Space();

            /**
                * Bind events
                */

            bool multiedit = serializedObject.targetObjects.Length > 1;

            if (multiedit) {
                EditorGUILayout.HelpBox("Events not available when editing multiple interactables!", MessageType.Warning, wide: true);
            } else {
                showEvents = EditorGUILayout.Foldout(showEvents, showEventsTxt);
                if (showEvents) {
                    EditorGUILayout.PropertyField(OnGrabProp);
                    EditorGUILayout.PropertyField(OnReleaseProp);
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Finger events", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(OnTouchProp);
                    EditorGUILayout.PropertyField(UnTouchProp);
                }
            }

            /**
                * Advanced config
                */
            showAdvConfig = EditorGUILayout.Foldout(showAdvConfig, showAdvConfigTxt);
            if (showAdvConfig) {
                PropertyField(UseRenderCenter);
                PropertyField(SendHapticsToWholeHand);
                EditorGUILayout.Space();

                PropertyField(maintainOrientation);
                PropertyField(maintainPosition);
                PropertyField(stayInHand);
                EditorGUILayout.Space();

                PropertyField(gripCollider);
                PropertyField(gripTransform);
            }

            // Apply property changes
            serializedObject.ApplyModifiedProperties();
        }

        private static bool PropertyField(SerializedProperty property)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            EditorGUILayout.PropertyField(property);
            EditorGUI.showMixedValue = false;
            return EditorGUI.EndChangeCheck();
        }
    }
}