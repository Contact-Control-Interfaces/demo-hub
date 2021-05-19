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

        private SerializedProperty tipSize, middleSize, knuckleSize;

        private SerializedProperty thumbTip, thumbMiddle, thumbKnuckle,
                                   indexTip, indexMiddle, indexKnuckle,
                                   middleTip, middleMiddle, middleKnuckle,
                                   ringTip, ringMiddle, ringKnuckle,
                                   littleTip, littleMiddle, littleKnuckle,
                                   palmBase;

        private SerializedProperty flatnessChecker;
        private SerializedProperty objectLayer;
        private SerializedProperty palmMeshWait;
        private SerializedProperty tooClose;
        private SerializedProperty tooFast;

        private SerializedProperty destroyFingerRenderersOnSpawn;
        private SerializedProperty showPalmMesh;

        public void OnEnable()
        {
            settingsOverride = this.serializedObject.FindProperty("settingsOverride");
            
            whichHand = this.serializedObject.FindProperty("whichHand");
            otherHand = this.serializedObject.FindProperty("otherHand");

            interactablesOnly = this.serializedObject.FindProperty("interactablesOnly");

            tipSize = this.serializedObject.FindProperty("TipSize");
            middleSize = this.serializedObject.FindProperty("MiddleSize");
            knuckleSize = this.serializedObject.FindProperty("KnuckleSize");


            //transforms, will be condensed into a struct in another PR
            thumbTip = this.serializedObject.FindProperty("ThumbTip");
            thumbMiddle = this.serializedObject.FindProperty("ThumbMiddle");
            thumbKnuckle = this.serializedObject.FindProperty("ThumbKnuckle");

            indexTip = this.serializedObject.FindProperty("IndexTip");
            indexMiddle = this.serializedObject.FindProperty("IndexMiddle");
            indexKnuckle = this.serializedObject.FindProperty("IndexKnuckle");

            middleTip = this.serializedObject.FindProperty("MiddleTip");
            middleMiddle = this.serializedObject.FindProperty("MiddleMiddle");
            middleKnuckle = this.serializedObject.FindProperty("MiddleKnuckle");

            ringTip = this.serializedObject.FindProperty("RingTip");
            ringMiddle = this.serializedObject.FindProperty("RingMiddle");
            ringKnuckle = this.serializedObject.FindProperty("RingKnuckle");

            littleTip = this.serializedObject.FindProperty("LittleTip");
            littleMiddle = this.serializedObject.FindProperty("LittleMiddle");
            littleKnuckle = this.serializedObject.FindProperty("LittleKnuckle");

            palmBase = this.serializedObject.FindProperty("PalmBase");

            flatnessChecker = this.serializedObject.FindProperty("flatnessChecker");
            objectLayer = this.serializedObject.FindProperty("objectLayer");
            palmMeshWait = this.serializedObject.FindProperty("palmMeshWait");
            tooClose = this.serializedObject.FindProperty("tooClose");
            tooFast = this.serializedObject.FindProperty("tooFast");

            destroyFingerRenderersOnSpawn = this.serializedObject.FindProperty("DestroyFingerRenderersOnSpawn");
            showPalmMesh = this.serializedObject.FindProperty("showPalmMesh");
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
                interactablesOnly.boolValue = EditorGUILayout.Toggle(new GUIContent("Interactables Only", "hover text"), hand.interactablesOnly);
                if (!hand.interactablesOnly)
                {
                    byte Amplitude = hand.defaultEffect.Amplitude;
                    byte Vibration = hand.defaultEffect.Vibration;
                    Amplitude = //
                        (byte)EditorGUILayout.IntSlider("Feedback Amplitude",
                        Amplitude,
                        HapticEffect.FORCE_FEEDBACK_MIN_AMPLITUDE,
                        HapticEffect.FORCE_FEEDBACK_MAX_AMPLITUDE);
                    Vibration =
                        (byte)EditorGUILayout.IntSlider("Vibration Effect",
                        Vibration,
                        HapticEffect.VIBRATION_MIN_ID,
                        HapticEffect.VIBRATION_MAX_ID);
                    EditorGUILayout.HelpBox(DRV2605Descriptions.get(hand.defaultEffect.Vibration), MessageType.None, false);

                    var childEnum = this.serializedObject.FindProperty("defaultEffect").GetEnumerator();
                    while (childEnum.MoveNext())
                    {
                        SerializedProperty current = childEnum.Current as SerializedProperty;
                        if (current.name.Equals("Amplitude"))
                            current.intValue = Amplitude;
                        else if (current.name.Equals("Vibration"))
                            current.intValue = Vibration;
                    }
                }
            }
            showHandConfig = EditorGUILayout.Foldout(showHandConfig, showHandConfigTxt);
            if (showHandConfig) {
                if (displayOverrideSettings)
                {
                    EditorGUILayout.LabelField("Hand Sizing", EditorStyles.boldLabel);
                    tipSize.floatValue = EditorGUILayout.FloatField("Tip Size", tipSize.floatValue);
                    middleSize.floatValue = EditorGUILayout.FloatField("Middle Size", middleSize.floatValue);
                    knuckleSize.floatValue = EditorGUILayout.FloatField("Knuckle Size", knuckleSize.floatValue);
                    EditorGUILayout.Space();
                }

                EditorGUILayout.LabelField("Positions on Hand", EditorStyles.boldLabel);
                thumbTip.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Thumb Tip", hand.ThumbTip, typeof(Transform), allowSceneObjects: true);
                thumbMiddle.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Thumb Middle", hand.ThumbMiddle, typeof(Transform), allowSceneObjects: true);
                thumbKnuckle.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Thumb Knuckle", hand.ThumbKnuckle, typeof(Transform), allowSceneObjects: true);
                EditorGUILayout.Space();
                indexTip.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Index Tip", hand.IndexTip, typeof(Transform), allowSceneObjects: true);
                indexMiddle.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Index Middle", hand.IndexMiddle, typeof(Transform), allowSceneObjects: true);
                indexKnuckle.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Index Knuckle", hand.IndexKnuckle, typeof(Transform), allowSceneObjects: true);
                EditorGUILayout.Space();
                middleTip.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Middle Tip", hand.MiddleTip, typeof(Transform), allowSceneObjects: true);
                middleMiddle.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Middle Middle", hand.MiddleMiddle, typeof(Transform), allowSceneObjects: true);
                middleKnuckle.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Middle Knuckle", hand.MiddleKnuckle, typeof(Transform), allowSceneObjects: true);
                EditorGUILayout.Space();
                ringTip.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Ring Tip", hand.RingTip, typeof(Transform), allowSceneObjects: true);
                ringMiddle.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Ring Middle", hand.RingMiddle, typeof(Transform), allowSceneObjects: true);
                ringKnuckle.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Ring Knuckle", hand.RingKnuckle, typeof(Transform), allowSceneObjects: true);
                EditorGUILayout.Space();
                littleTip.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Little Tip", hand.LittleTip, typeof(Transform), allowSceneObjects: true);
                littleMiddle.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Little Middle", hand.LittleMiddle, typeof(Transform), allowSceneObjects: true);
                littleKnuckle.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Little Knuckle", hand.LittleKnuckle, typeof(Transform), allowSceneObjects: true);
                EditorGUILayout.Space();
                palmBase.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Palm Base", hand.PalmBase, typeof(Transform), allowSceneObjects: true);
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
                EditorGUILayout.LabelField("Dist 1", hand.dist1.ToString("F3"));
                EditorGUILayout.LabelField("Dist 2", hand.dist2.ToString("F3"));
                EditorGUILayout.LabelField("Ratio 1", hand.ratio1.ToString("F3"));
                EditorGUILayout.LabelField("Ratio 2", hand.ratio2.ToString("F3"));
            }
            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
