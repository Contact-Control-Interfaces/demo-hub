using Maestro.EditorExtensions;
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
    [CustomEditor(typeof(MaestroHandV2))]
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

        private SerializedProperty destroyFingerRenderersOnSpawn;
        private SerializedProperty showPalmMesh;

        private SerializedProperty defaultEffect;
        private SerializedProperty handSize;
        private SerializedProperty transforms;

        public void OnEnable()
        {

            settingsOverride = this.serializedObject.FindProperty("settingsOverride");
            
            whichHand = this.serializedObject.FindProperty("whichHand");
            otherHand = this.serializedObject.FindProperty("otherHand");

            interactablesOnly = this.serializedObject.FindProperty("interactablesOnly");

            flatnessChecker = this.serializedObject.FindProperty("flatnessChecker");
            objectLayer = this.serializedObject.FindProperty("objectLayer");
            palmMeshWait = this.serializedObject.FindProperty("palmMeshWait");
            tooClose = this.serializedObject.FindProperty("tooClose");
            tooFast = this.serializedObject.FindProperty("tooFast");

            destroyFingerRenderersOnSpawn = this.serializedObject.FindProperty("DestroyFingerRenderersOnSpawn");
            showPalmMesh = this.serializedObject.FindProperty("showPalmMesh");

            defaultEffect = this.serializedObject.FindProperty("defaultEffect");
            handSize = this.serializedObject.FindProperty("handSize");
            transforms = this.serializedObject.FindProperty("transforms");
        }

        public override void OnInspectorGUI()
        {
            MaestroHandV2 hand = (MaestroHandV2)target;

            EditorGUILayout.LabelField("Main Configuration", EditorStyles.boldLabel);
            settingsOverride.boolValue = EditorGUILayout.Toggle("Override Settings", settingsOverride.boolValue);

            bool displayOverrideSettings = hand.manager == null ||
                hand != ((hand.whichHand == WhichHand.RightHand) ? hand.manager.RightHand : hand.manager.LeftHand) ||
                hand.settingsOverride;

            if (displayOverrideSettings && !hand.settingsOverride)
                EditorGUILayout.HelpBox("Not attached to a manager, will not cascade settings.", MessageType.Info, wide: true);

            if (displayOverrideSettings)
            {
                EditorGUILayout.PropertyField(whichHand);
                otherHand.objectReferenceValue = (MaestroHandV2)EditorGUILayout.ObjectField("Other Hand", otherHand.objectReferenceValue, typeof(MaestroHandV2), allowSceneObjects: true);
            }  
            EditorGUILayout.Space();

            bool handNotFullyDefined =
                hand.transforms.ThumbTip == null || hand.transforms.ThumbMiddle == null || hand.transforms.ThumbKnuckle == null ||
                hand.transforms.IndexTip == null || hand.transforms.IndexMiddle == null || hand.transforms.IndexKnuckle == null ||
                hand.transforms.MiddleTip == null || hand.transforms.MiddleMiddle == null || hand.transforms.MiddleKnuckle == null ||
                hand.transforms.RingTip == null || hand.transforms.RingMiddle == null || hand.transforms.RingKnuckle == null ||
                hand.transforms.LittleTip == null || hand.transforms.LittleMiddle == null || hand.transforms.LittleKnuckle == null ||
                hand.transforms.PalmBase == null;
            
            if (hand.otherHand == null) {
                EditorGUILayout.HelpBox("Other hand not defined! Two-handed interactions will disabled!", MessageType.Warning, wide: true);
            }

            if (handNotFullyDefined) {
                EditorGUILayout.HelpBox("Hand not fully defined!", MessageType.Error, wide: true);
            }

            // Read-only toggles for grab states
            EditorGUILayout.Toggle("Is Grabbing", hand.grabbing);
            EditorGUILayout.Toggle("Is Two-Hand Grabbing", hand.twoHandGrabbing);
            
            EditorGUILayout.Space();

            if (displayOverrideSettings)
            {
                EditorGUILayout.LabelField("Default Interaction Profile", EditorStyles.boldLabel);
                interactablesOnly.boolValue = EditorGUILayout.Toggle(new GUIContent("Interactables Only", "hover text"), hand.interactablesOnly);
                if (!hand.interactablesOnly)
                {

                    defaultEffect.FindPropertyRelative("Amplitude").intValue = 
                        (byte)EditorGUILayout.IntSlider("Feedback Amplitude",
                        hand.defaultEffect.Amplitude,
                        HapticEffect.FORCE_FEEDBACK_MIN_AMPLITUDE,
                        HapticEffect.FORCE_FEEDBACK_MAX_AMPLITUDE);
                    defaultEffect.FindPropertyRelative("Vibration").intValue =
                        (byte)EditorGUILayout.IntSlider("Vibration Effect",
                        hand.defaultEffect.Vibration,
                        HapticEffect.VIBRATION_MIN_ID,
                        HapticEffect.VIBRATION_MAX_ID);
                    EditorGUILayout.HelpBox(DRV2605Descriptions.get(hand.defaultEffect.Vibration), MessageType.None, false);
                }
            }
            showHandConfig = EditorGUILayout.Foldout(showHandConfig, showHandConfigTxt);
            if (showHandConfig) {
                if (displayOverrideSettings)
                {
                    EditorGUILayout.LabelField("Hand Sizing", EditorStyles.boldLabel);
                    handSize.FindPropertyRelative("TipSize").floatValue = EditorGUILayout.FloatField("Tip Size", hand.handSize.TipSize);
                    handSize.FindPropertyRelative("MiddleSize").floatValue = EditorGUILayout.FloatField("Middle Size", hand.handSize.MiddleSize);
                    handSize.FindPropertyRelative("KnuckleSize").floatValue = EditorGUILayout.FloatField("Knuckle Size", hand.handSize.KnuckleSize);

                    EditorGUILayout.Space();
                }

                EditorGUILayout.LabelField("Positions on Hand", EditorStyles.boldLabel);
                transforms.FindPropertyRelative("ThumbTip").objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Thumb Tip", hand.transforms.ThumbTip, typeof(Transform), allowSceneObjects: true);
                transforms.FindPropertyRelative("ThumbMiddle").objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Thumb Middle", hand.transforms.ThumbMiddle, typeof(Transform), allowSceneObjects: true);
                transforms.FindPropertyRelative("ThumbKnuckle").objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Thumb Knuckle", hand.transforms.ThumbKnuckle, typeof(Transform), allowSceneObjects: true);
                EditorGUILayout.Space();
                transforms.FindPropertyRelative("IndexTip").objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Index Tip", hand.transforms.IndexTip, typeof(Transform), allowSceneObjects: true);
                transforms.FindPropertyRelative("IndexMiddle").objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Index Middle", hand.transforms.IndexMiddle, typeof(Transform), allowSceneObjects: true);
                transforms.FindPropertyRelative("IndexKnuckle").objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Index Knuckle", hand.transforms.IndexKnuckle, typeof(Transform), allowSceneObjects: true);
                EditorGUILayout.Space();
                transforms.FindPropertyRelative("MiddleTip").objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Middle Tip", hand.transforms.MiddleTip, typeof(Transform), allowSceneObjects: true);
                transforms.FindPropertyRelative("MiddleMiddle").objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Middle Middle", hand.transforms.MiddleMiddle, typeof(Transform), allowSceneObjects: true);
                transforms.FindPropertyRelative("MiddleKnuckle").objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Middle Knuckle", hand.transforms.MiddleKnuckle, typeof(Transform), allowSceneObjects: true);
                EditorGUILayout.Space();
                transforms.FindPropertyRelative("RingTip").objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Ring Tip", hand.transforms.RingTip, typeof(Transform), allowSceneObjects: true);
                transforms.FindPropertyRelative("RingMiddle").objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Ring Middle", hand.transforms.RingMiddle, typeof(Transform), allowSceneObjects: true);
                transforms.FindPropertyRelative("RingKnuckle").objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Ring Knuckle", hand.transforms.RingKnuckle, typeof(Transform), allowSceneObjects: true);
                EditorGUILayout.Space();
                transforms.FindPropertyRelative("LittleTip").objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Little Tip", hand.transforms.LittleTip, typeof(Transform), allowSceneObjects: true);
                transforms.FindPropertyRelative("LittleMiddle").objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Little Middle", hand.transforms.LittleMiddle, typeof(Transform), allowSceneObjects: true);
                transforms.FindPropertyRelative("LittleKnuckle").objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Little Knuckle", hand.transforms.LittleKnuckle, typeof(Transform), allowSceneObjects: true);
                EditorGUILayout.Space();
                transforms.FindPropertyRelative("PalmBase").objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Palm Base", hand.transforms.PalmBase, typeof(Transform), allowSceneObjects: true);
            }

            if (displayOverrideSettings)
                {
                showAdvConfig = EditorGUILayout.Foldout(showAdvConfig, showAdvConfigTxt);
                if (showAdvConfig)
                {
                    EditorGUILayout.LabelField("Optional", EditorStyles.boldLabel);
                    flatnessChecker.objectReferenceValue = (FlatnessChecker)EditorGUILayout.ObjectField("Flatness Checker", hand.flatnessChecker, typeof(FlatnessChecker), allowSceneObjects: true);
                    objectLayer.intValue = EditorGUILayout.LayerField("Object Layer", hand.objectLayer);
                    EditorGUILayout.Space();

                    palmMeshWait.floatValue = EditorGUILayout.FloatField("Palm Mesh Generation Tick", hand.palmMeshWait);
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Finger Collider Reset", EditorStyles.boldLabel);
                    tooClose.floatValue = EditorGUILayout.FloatField("Too Close Threshold", hand.tooClose);
                    tooFast.floatValue = EditorGUILayout.FloatField("Too Fast Threshold", hand.tooFast);
                }
            }
            showDebug = EditorGUILayout.Foldout(showDebug, showDebugTxt);
            if (showDebug) {
                destroyFingerRenderersOnSpawn.boolValue = EditorGUILayout.Toggle("Destroy Finger Renderers on Spawn", hand.DestroyFingerRenderersOnSpawn);
                showPalmMesh.boolValue = EditorGUILayout.Toggle("Show Palm Meshes", hand.showPalmMesh);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Time Since Last Grab", hand.timeSinceGrabbing.ToString("F3"));
                EditorGUILayout.LabelField("Time Since Last Release", hand.timeSinceRelease.ToString("F3"));
                EditorGUILayout.LabelField("Time Since Last Two-Hand Grab", hand.timeSinceTwoHandGrabbing.ToString("F3"));
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("F1", hand.f1.ToString());
                EditorGUILayout.LabelField("F2", hand.f2.ToString());
                //EditorGUILayout.LabelField("Dist 1", hand.dist1.ToString("F3"));
                //EditorGUILayout.LabelField("Dist 2", hand.dist2.ToString("F3"));
                EditorGUILayout.LabelField("Ratio 1", hand.ratio1.ToString("F3"));
                EditorGUILayout.LabelField("Ratio 2", hand.ratio2.ToString("F3"));
            }
            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
