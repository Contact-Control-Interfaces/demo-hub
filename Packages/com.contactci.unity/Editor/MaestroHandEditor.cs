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
    [CustomEditor(typeof(MaestroHand))]
    public class MaestroHandV2Editor : Editor
    {
        private bool showHandConfig = false;
        private string showHandConfigTxt = "Hand Definition";

        private bool showAdvConfig = false;
        private string showAdvConfigTxt = "Advanced Config";

        private bool showDebug = false;
        private string showDebugTxt = "Debug";

        private SerializedProperty settingsOverride;

        private SerializedProperty whichHand;
        private SerializedProperty otherHand;

        SerializedProperty interactablesOnly;

        private SerializedProperty flatnessChecker;
        private SerializedProperty objectLayer;
        private SerializedProperty palmMeshWait;
        private SerializedProperty tooClose;
        private SerializedProperty tooFast;

        private SerializedProperty destroyRenderersOnSpawn;
        private SerializedProperty renderOnTop;
        private SerializedProperty showPalmMesh;

        private SerializedProperty defaultEffect;
        private SerializedProperty handSize;
        private SerializedProperty transforms;
        private SerializedProperty grabType;
        private SerializedProperty showWhileTouching;

        public void OnEnable()
        {
            settingsOverride = this.serializedObject.FindProperty("settingsOverride");
            
            whichHand = this.serializedObject.FindProperty("whichHand");
            otherHand = this.serializedObject.FindProperty("otherHandOverride");

            interactablesOnly = this.serializedObject.FindProperty("interactablesOnlyOverride");

            flatnessChecker = this.serializedObject.FindProperty("flatnessCheckerOverride");
            objectLayer = this.serializedObject.FindProperty("objectLayerOverride");
            palmMeshWait = this.serializedObject.FindProperty("palmMeshWaitOverride");
            tooClose = this.serializedObject.FindProperty("tooCloseOverride");
            tooFast = this.serializedObject.FindProperty("tooFastOverride");

            defaultEffect = this.serializedObject.FindProperty("defaultEffectOverride");
            handSize = this.serializedObject.FindProperty("handSizeOverride");
            transforms = this.serializedObject.FindProperty("transforms");


            destroyRenderersOnSpawn = this.serializedObject.FindProperty("DestroyRenderersOnSpawn");
            renderOnTop = this.serializedObject.FindProperty("RenderOnTop");

            showPalmMesh = this.serializedObject.FindProperty("inflatePalm");
            grabType = this.serializedObject.FindProperty("grabTypeOverride");
            showWhileTouching = this.serializedObject.FindProperty("ShowOnlyWhileTouching");
        }

        public override void OnInspectorGUI()
        {
            MaestroHand hand = (MaestroHand)target;

            EditorGUILayout.LabelField("Main Configuration", EditorStyles.boldLabel);
            settingsOverride.boolValue = EditorGUILayout.Toggle("Override Settings", settingsOverride.boolValue);
            settingsOverride.serializedObject.ApplyModifiedProperties();
            
            bool displayOverrideSettings = hand.manager == null ||
                hand != ((hand.whichHand == WhichHand.RightHand) ? hand.manager.RightHand : hand.manager.LeftHand) ||
                hand.settingsOverride;

            if (displayOverrideSettings) {
                if (!hand.settingsOverride)
                        EditorGUILayout.HelpBox("Not attached to a manager, will not inherit settings.", MessageType.Info, wide: true);

                EditorGUILayout.PropertyField(whichHand);
                otherHand.objectReferenceValue = (MaestroHand)EditorGUILayout.ObjectField("Other Hand", otherHand.objectReferenceValue, typeof(MaestroHand), allowSceneObjects: true);
            }
            EditorGUILayout.Space();
            
            if (hand.otherHand == null) {
                EditorGUILayout.HelpBox("Other hand not defined! Two-handed interactions will disabled!", MessageType.Warning, wide: true);
            }

            if (!hand.transforms.FullyDefined) {
                EditorGUILayout.HelpBox("Hand not fully defined!", MessageType.Error, wide: true);
            }

            // Read-only toggles for grab states
            EditorGUILayout.Toggle("Is Grabbing", hand.grabbing);
            EditorGUILayout.Toggle("Is Two-Hand Grabbing", hand.twoHandGrabbing);
            
            EditorGUILayout.Space();

            if (displayOverrideSettings) {
                EditorGUILayout.LabelField("Default Interaction Profile", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(grabType, new GUIContent("Grab Type", "Controls how objects are picked up and manipulated"));

                interactablesOnly.boolValue = EditorGUILayout.Toggle(new GUIContent("Interactables Only", "hover text"), hand.interactablesOnlyOverride);
                if (!hand.interactablesOnlyOverride) {
                    EditorGUILayout.PropertyField(defaultEffect, new GUIContent("Default Effect"));
                    EditorGUILayout.Space();
                }
            }

            showHandConfig = EditorGUILayout.Foldout(showHandConfig, showHandConfigTxt);
            if (showHandConfig) {
                if (displayOverrideSettings) {
                    EditorGUILayout.PropertyField(handSize, new GUIContent("Hand Sizing"));
                    EditorGUILayout.Space();
                }

                EditorGUILayout.PropertyField(transforms, new GUIContent("Positions on Hand"));
            }

            if (displayOverrideSettings) {
                showAdvConfig = EditorGUILayout.Foldout(showAdvConfig, showAdvConfigTxt);
                if (showAdvConfig) {
                    EditorGUILayout.LabelField("Optional", EditorStyles.boldLabel);
                    flatnessChecker.objectReferenceValue = (FlatnessChecker)EditorGUILayout.ObjectField("Flatness Checker", hand.flatnessCheckerOverride, typeof(FlatnessChecker), allowSceneObjects: true);
                    objectLayer.intValue = EditorGUILayout.LayerField("Object Layer", hand.objectLayerOverride);
                    EditorGUILayout.Space();

                    palmMeshWait.floatValue = EditorGUILayout.FloatField("Palm Mesh Generation Tick", hand.palmMeshWaitOverride);
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Finger Collider Reset", EditorStyles.boldLabel);
                    tooClose.floatValue = EditorGUILayout.FloatField("Too Close Threshold", hand.tooCloseOverride);
                    tooFast.floatValue = EditorGUILayout.FloatField("Too Fast Threshold", hand.tooFastOverride);
                }
            }

            showDebug = EditorGUILayout.Foldout(showDebug, showDebugTxt);
            if (showDebug) {
                destroyRenderersOnSpawn.boolValue = EditorGUILayout.Toggle("Destroy Renderers on Spawn", hand.DestroyRenderersOnSpawn);
                renderOnTop.boolValue = EditorGUILayout.Toggle("Render on Top", hand.RenderOnTop);
                showPalmMesh.boolValue = EditorGUILayout.Toggle("Inflate Palm Meshes", hand.inflatePalm);
                showWhileTouching.boolValue = EditorGUILayout.Toggle("Show Only While Touching", hand.ShowOnlyWhileTouching);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Time Since Last Two-Hand Grab", hand.timeSinceTwoHandGrabbing.ToString("F3"));
                EditorGUILayout.Space();
            }
            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
