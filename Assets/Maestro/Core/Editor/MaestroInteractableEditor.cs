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

        private void OnEnable()
        {
            OnTouchProp = serializedObject.FindProperty("onTouch");
            UnTouchProp = serializedObject.FindProperty("unTouch");
            OnGrabProp = serializedObject.FindProperty("onGrab");
            OnReleaseProp = serializedObject.FindProperty("onRelease");
        }

        public override void OnInspectorGUI()
        {
            MaestroInteractable mi = (MaestroInteractable) target;

            /** 
             * Haptics
             */
            EditorGUILayout.LabelField("Haptic Configuration", EditorStyles.boldLabel);

            mi.Amplitude = (byte)EditorGUILayout.IntSlider("Amplitude", mi.Amplitude, 0, 255);
            mi.VibrationEffect = (byte)EditorGUILayout.IntSlider("Vibration Effect", mi.VibrationEffect, 0, 128);
            // Describe DRV2605 effect below
            EditorGUILayout.HelpBox(DRV2605Descriptions.get(mi.VibrationEffect), MessageType.None, false);
            EditorGUILayout.Space();

            /**
             * Configuration
             */
            mi.type = (InteractionType) EditorGUILayout.EnumPopup("Type", mi.type);
            mi.IgnoreTaps = EditorGUILayout.Toggle("Ignore Taps", mi.IgnoreTaps);
            mi.isPersistent = EditorGUILayout.Toggle("Persist", mi.isPersistent);
            if (mi.isPersistent) {
                mi.persistanceDuration = EditorGUILayout.FloatField("Persistence Duration", mi.persistanceDuration);
            }
            EditorGUILayout.Space();

            /**
             * Bind events
             */
            showEvents = EditorGUILayout.Foldout(showEvents, showEventsTxt);
            if (showEvents) {
                EditorGUILayout.PropertyField(OnGrabProp);
                EditorGUILayout.PropertyField(OnReleaseProp);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Finger events", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(OnTouchProp);
                EditorGUILayout.PropertyField(UnTouchProp);
            }

            /**
             * Advanced config
             */
            showAdvConfig = EditorGUILayout.Foldout(showAdvConfig, showAdvConfigTxt);
            if (showAdvConfig) {
                mi.UseRenderCenter = EditorGUILayout.Toggle("Use Render Center", mi.UseRenderCenter);
                mi.SendHapticsToWholeHand = EditorGUILayout.Toggle("Send Haptics to Whole Hand", mi.SendHapticsToWholeHand);
                EditorGUILayout.Space();

                mi.maintainOrientation = EditorGUILayout.Toggle("Maintain Orientation", mi.maintainOrientation);
                mi.maintainPosition = EditorGUILayout.Toggle("Maintain Position", mi.maintainPosition);
                mi.stayInHand = EditorGUILayout.Toggle("Stay in hand", mi.stayInHand);
                EditorGUILayout.Space();

                mi.gripCollider = EditorGUILayout.ObjectField("Grip Collider", mi.gripCollider, typeof(Collider), allowSceneObjects: true) as Collider;
                mi.gripTransform = EditorGUILayout.ObjectField("Grip Transform", mi.gripTransform, typeof(Transform), allowSceneObjects: true) as Transform;
            }

            // Apply property changes
            serializedObject.ApplyModifiedProperties();
        }
    }
}