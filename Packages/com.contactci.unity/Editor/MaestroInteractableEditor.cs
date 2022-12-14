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

        private SerializedProperty OnTouchProp, UnTouchProp, WhileTouchProp, OnGrabProp, OnReleaseProp;
        private SerializedProperty startHaptics, stayHaptics, exitHaptics;
        private SerializedProperty type, ignoreTaps, persist, persistenceDuration;
        private SerializedProperty UseRenderCenter, maintainOrientation, maintainPosition, stayInHand, SendHapticsToWholeHand;
        private SerializedProperty gripTransform, gripCollider;
        private SerializedProperty triggerTouch, triggerUntouch, triggerStay;

        private void OnEnable()
        {
            OnTouchProp = serializedObject.FindProperty("onTouch");
            UnTouchProp = serializedObject.FindProperty("unTouch");
            WhileTouchProp = serializedObject.FindProperty("whileTouch");
            OnGrabProp = serializedObject.FindProperty("onGrab");
            OnReleaseProp = serializedObject.FindProperty("onRelease");
            startHaptics = serializedObject.FindProperty("startHaptics");
            stayHaptics = serializedObject.FindProperty("stayHaptics");
            exitHaptics = serializedObject.FindProperty("exitHaptics");

            triggerTouch = serializedObject.FindProperty("onTriggerTouch");
            triggerUntouch = serializedObject.FindProperty("unTriggerTouch");
            triggerStay = serializedObject.FindProperty("whileTriggerTouch");

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
            EditorGUILayout.PropertyField(startHaptics);
            EditorGUILayout.PropertyField(stayHaptics);
            EditorGUILayout.PropertyField(exitHaptics);
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
                int eventsBound = mi.onGrab.GetPersistentEventCount() +
                  mi.onRelease.GetPersistentEventCount() +
                  mi.onTouch.GetPersistentEventCount() +
                  mi.unTouch.GetPersistentEventCount() +
                  mi.whileTouch.GetPersistentEventCount() +
                  mi.onTriggerTouch.GetPersistentEventCount() +
                  mi.unTriggerTouch.GetPersistentEventCount() +
                  mi.whileTriggerTouch.GetPersistentEventCount();
                bool areBound = eventsBound > 0;

                showEvents = EditorGUILayout.Foldout(showEvents, showEventsTxt + (areBound?string.Format("({0}) ", eventsBound):""), 
                    areBound?EditorStyles.foldoutHeader:EditorStyles.foldout);
                if (showEvents) {
                    EditorGUILayout.PropertyField(OnGrabProp);
                    EditorGUILayout.PropertyField(OnReleaseProp);
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Finger events", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(OnTouchProp);
                    EditorGUILayout.PropertyField(UnTouchProp);
                    EditorGUILayout.PropertyField(WhileTouchProp);
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Finger Trigger events", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(triggerTouch);
                    EditorGUILayout.PropertyField(triggerUntouch);
                    EditorGUILayout.PropertyField(triggerStay);
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