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


        public void OnEnable()
        {
            /* Effectively the inspector's OnStart */
        }

        public override void OnInspectorGUI()
        {
            MaestroHandV2 hand = (MaestroHandV2)target;

            EditorGUILayout.LabelField("Main Configuration", EditorStyles.boldLabel);
            hand.settingsOverride = EditorGUILayout.Toggle("Override Settings", hand.settingsOverride);

            bool displayOverrideSettings = hand.manager == null ||
                hand != ((hand.whichHand == WhichHand.RightHand) ? hand.manager.RightHand : hand.manager.LeftHand) ||
                hand.settingsOverride;

            if (displayOverrideSettings && !hand.settingsOverride)
                EditorGUILayout.HelpBox("Not attached to a manager, will not cascade settings.", MessageType.Info, wide: true);

            if (displayOverrideSettings)
            {
                hand.whichHand = (WhichHand)EditorGUILayout.EnumPopup("Which Hand", hand.whichHand);
                hand.otherHand = (MaestroHandV2)EditorGUILayout.ObjectField("Other Hand", hand.otherHand, typeof(MaestroHandV2), allowSceneObjects: true);
            }  
            EditorGUILayout.Space();

            bool handNotFullyDefined =
                hand.ThumbTip == null || hand.ThumbMiddle == null || hand.ThumbKnuckle == null ||
                hand.IndexTip == null || hand.IndexMiddle == null || hand.IndexKnuckle == null ||
                hand.MiddleTip == null || hand.MiddleMiddle == null || hand.MiddleKnuckle == null ||
                hand.RingTip == null || hand.RingMiddle == null || hand.RingKnuckle == null ||
                hand.LittleTip == null || hand.LittleMiddle == null || hand.LittleKnuckle == null ||
                hand.PalmBase == null;
            
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
                hand.interactablesOnly = EditorGUILayout.Toggle(new GUIContent("Interactables Only", "hover text"), hand.interactablesOnly);
                if (!hand.interactablesOnly)
                {
                    hand.defaultEffect.Amplitude = (byte)EditorGUILayout.IntSlider("Feedback Amplitude", 
                        hand.defaultEffect.Amplitude, 
                        HapticEffect.FORCE_FEEDBACK_MIN_AMPLITUDE, 
                        HapticEffect.FORCE_FEEDBACK_MAX_AMPLITUDE);
                    hand.defaultEffect.Vibration = (byte)EditorGUILayout.IntSlider("Vibration Effect", 
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
                    hand.TipSize = EditorGUILayout.FloatField("Tip Size", hand.TipSize);
                    hand.MiddleSize = EditorGUILayout.FloatField("Middle Size", hand.MiddleSize);
                    hand.KnuckleSize = EditorGUILayout.FloatField("Knuckle Size", hand.KnuckleSize);
                    EditorGUILayout.Space();
                }

                EditorGUILayout.LabelField("Positions on Hand", EditorStyles.boldLabel);
                hand.ThumbTip = (Transform)EditorGUILayout.ObjectField("Thumb Tip", hand.ThumbTip, typeof(Transform), allowSceneObjects: true);
                hand.ThumbMiddle = (Transform)EditorGUILayout.ObjectField("Thumb Middle", hand.ThumbMiddle, typeof(Transform), allowSceneObjects: true);
                hand.ThumbKnuckle = (Transform)EditorGUILayout.ObjectField("Thumb Knuckle", hand.ThumbKnuckle, typeof(Transform), allowSceneObjects: true);
                EditorGUILayout.Space();
                hand.IndexTip = (Transform)EditorGUILayout.ObjectField("Index Tip", hand.IndexTip, typeof(Transform), allowSceneObjects: true);
                hand.IndexMiddle = (Transform)EditorGUILayout.ObjectField("Index Middle", hand.IndexMiddle, typeof(Transform), allowSceneObjects: true);
                hand.IndexKnuckle = (Transform)EditorGUILayout.ObjectField("Index Knuckle", hand.IndexKnuckle, typeof(Transform), allowSceneObjects: true);
                EditorGUILayout.Space();
                hand.MiddleTip = (Transform)EditorGUILayout.ObjectField("Middle Tip", hand.MiddleTip, typeof(Transform), allowSceneObjects: true);
                hand.MiddleMiddle = (Transform)EditorGUILayout.ObjectField("Middle Middle", hand.MiddleMiddle, typeof(Transform), allowSceneObjects: true);
                hand.MiddleKnuckle = (Transform)EditorGUILayout.ObjectField("Middle Knuckle", hand.MiddleKnuckle, typeof(Transform), allowSceneObjects: true);
                EditorGUILayout.Space();
                hand.RingTip = (Transform)EditorGUILayout.ObjectField("Ring Tip", hand.RingTip, typeof(Transform), allowSceneObjects: true);
                hand.RingMiddle = (Transform)EditorGUILayout.ObjectField("Ring Middle", hand.RingMiddle, typeof(Transform), allowSceneObjects: true);
                hand.RingKnuckle = (Transform)EditorGUILayout.ObjectField("Ring Knuckle", hand.RingKnuckle, typeof(Transform), allowSceneObjects: true);
                EditorGUILayout.Space();
                hand.LittleTip = (Transform)EditorGUILayout.ObjectField("Little Tip", hand.LittleTip, typeof(Transform), allowSceneObjects: true);
                hand.LittleMiddle = (Transform)EditorGUILayout.ObjectField("Little Middle", hand.LittleMiddle, typeof(Transform), allowSceneObjects: true);
                hand.LittleKnuckle = (Transform)EditorGUILayout.ObjectField("Little Knuckle", hand.LittleKnuckle, typeof(Transform), allowSceneObjects: true);
                EditorGUILayout.Space();
                hand.PalmBase = (Transform)EditorGUILayout.ObjectField("Palm Base", hand.PalmBase, typeof(Transform), allowSceneObjects: true);
            }

            if (displayOverrideSettings)
                {
                showAdvConfig = EditorGUILayout.Foldout(showAdvConfig, showAdvConfigTxt);
                if (showAdvConfig)
                {
                    EditorGUILayout.LabelField("Optional", EditorStyles.boldLabel);
                    hand.flatnessChecker = (FlatnessChecker)EditorGUILayout.ObjectField("Flatness Checker", hand.flatnessChecker, typeof(FlatnessChecker), allowSceneObjects: true);
                    hand.objectLayer = EditorGUILayout.LayerField("Object Layer", hand.objectLayer);
                    EditorGUILayout.Space();

                    hand.palmMeshWait = EditorGUILayout.FloatField("Palm Mesh Generation Tick", hand.palmMeshWait);
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Finger Collider Reset", EditorStyles.boldLabel);
                    hand.tooClose = EditorGUILayout.FloatField("Too Close Threshold", hand.tooClose);
                    hand.tooFast = EditorGUILayout.FloatField("Too Fast Threshold", hand.tooFast);
                }
            }
            showDebug = EditorGUILayout.Foldout(showDebug, showDebugTxt);
            if (showDebug) {
                hand.DestroyFingerRenderersOnSpawn = EditorGUILayout.Toggle("Destroy Finger Renderers on Spawn", hand.DestroyFingerRenderersOnSpawn);
                hand.showPalmMesh = EditorGUILayout.Toggle("Show Palm Meshes", hand.showPalmMesh);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Time Since Last Grab", hand.timeSinceGrabbing.ToString("F3"));
                EditorGUILayout.LabelField("Time Since Last Release", hand.timeSinceRelease.ToString("F3"));
                EditorGUILayout.LabelField("Time Since Last Two-Hand Grab", hand.timeSinceTwoHandGrabbing.ToString("F3"));
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("F1", hand.f1.ToString());
                EditorGUILayout.LabelField("F2", hand.f2.ToString());
                EditorGUILayout.LabelField("Dist 1", hand.dist1.ToString("F3"));
                EditorGUILayout.LabelField("Dist 2", hand.dist2.ToString("F3"));
                EditorGUILayout.LabelField("Ratio 1", hand.ratio1.ToString("F3"));
                EditorGUILayout.LabelField("Ratio 2", hand.ratio2.ToString("F3"));
            }
        }
    }
}
