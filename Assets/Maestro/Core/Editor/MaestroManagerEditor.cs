using Maestro.Vibration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Maestro
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MaestroManager))]
    public class MaestroManagerEditor : Editor
    {
        private bool showHandConfig = false;
        private string showHandConfigTxt = "Hand Definition";

        private bool showAdvConfig = false;
        private string showAdvConfigTxt = "Advanced Config";

        private SerializedProperty leftHand;
        private SerializedProperty rightHand;

        private SerializedProperty grabType;
        private SerializedProperty interactablesOnly;

        private SerializedProperty flatnessChecker;
        private SerializedProperty objectLayer;
        private SerializedProperty palmMeshWait;
        private SerializedProperty tooClose;
        private SerializedProperty tooFast;
        private SerializedProperty handSize;
        private SerializedProperty defaultEffect;

        public void OnEnable()
        {
            leftHand = this.serializedObject.FindProperty("LeftHand");
            rightHand = this.serializedObject.FindProperty("RightHand");
            
            interactablesOnly = this.serializedObject.FindProperty("InteractablesOnly");

            flatnessChecker = this.serializedObject.FindProperty("flatnessChecker");
            tooClose = this.serializedObject.FindProperty("tooClose");
            tooFast = this.serializedObject.FindProperty("tooFast");
            
            handSize = this.serializedObject.FindProperty("handSize");
            defaultEffect = this.serializedObject.FindProperty("DefaultEffect");

            grabType = this.serializedObject.FindProperty("grabType");
        }

        public override void OnInspectorGUI()
        {
            MaestroManager manager = (MaestroManager)target;

            EditorGUILayout.LabelField("Main Configuration2", EditorStyles.boldLabel);
            leftHand.objectReferenceValue = (MaestroHand)EditorGUILayout.ObjectField("Left Hand", manager.LeftHand, typeof(MaestroHand), allowSceneObjects: true);
            rightHand.objectReferenceValue = (MaestroHand)EditorGUILayout.ObjectField("Right Hand", manager.RightHand, typeof(MaestroHand), allowSceneObjects: true);
            EditorGUILayout.Space();


            if (manager.LeftHand == null && manager.RightHand == null) {
                EditorGUILayout.HelpBox("Hands not defined! Settings cannot yet be configured!", MessageType.Warning, wide: true);
            }  else if (manager.LeftHand == null) {
                EditorGUILayout.HelpBox("Left hand not defined! Two-handed interactions will disabled!", MessageType.Warning, wide: true);
            } else if (manager.RightHand == null) {
                EditorGUILayout.HelpBox("Right hand not defined! Two-handed interactions will disabled!", MessageType.Warning, wide: true);
            }

            if (manager.LeftHand != null && manager.LeftHand.settingsOverride)
                EditorGUILayout.HelpBox("Left hand will not take these settings because override is on.", MessageType.Info, wide: true);
            if (manager.RightHand != null && manager.RightHand.settingsOverride)
                EditorGUILayout.HelpBox("Right hand will not take these settings because override is on.", MessageType.Info, wide: true);


            if (manager.LeftHand != null || manager.RightHand != null) {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Default Interaction Profile", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(grabType, new GUIContent("Grab Type", "Controls how objects are picked up and manipulated"));

                EditorGUILayout.PropertyField(interactablesOnly, new GUIContent("Interactables Only", "Will only play effects from MaestroInteractables"));
                if (!manager.InteractablesOnly) {
                    EditorGUILayout.PropertyField(defaultEffect);
                    EditorGUILayout.Space();
                }

                showHandConfig = EditorGUILayout.Foldout(showHandConfig, showHandConfigTxt);
                if (showHandConfig) {
                    EditorGUILayout.PropertyField(handSize, new GUIContent("Hand Sizing"));
                    EditorGUILayout.Space();
                }

                showAdvConfig = EditorGUILayout.Foldout(showAdvConfig, showAdvConfigTxt);
                if (showAdvConfig) {
                    EditorGUILayout.LabelField("Optional", EditorStyles.boldLabel);
                    flatnessChecker.objectReferenceValue = (FlatnessChecker)EditorGUILayout.ObjectField("Flatness Checker", manager.flatnessChecker, typeof(FlatnessChecker), allowSceneObjects: true);
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Finger Collider Reset", EditorStyles.boldLabel);
                    tooClose.floatValue = EditorGUILayout.FloatField("Too Close Threshold", manager.tooClose);
                    tooFast.floatValue = EditorGUILayout.FloatField("Too Fast Threshold", manager.tooFast);
                }
            }

            manager.attachToHands();
            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
