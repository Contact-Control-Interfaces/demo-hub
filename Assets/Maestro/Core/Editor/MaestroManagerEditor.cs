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
            objectLayer = this.serializedObject.FindProperty("objectLayer");
            palmMeshWait = this.serializedObject.FindProperty("palmMeshWait");
            tooClose = this.serializedObject.FindProperty("tooClose");
            tooFast = this.serializedObject.FindProperty("tooFast");
            
            handSize = this.serializedObject.FindProperty("handSize");
            defaultEffect = this.serializedObject.FindProperty("DefaultEffect");
        }

        public override void OnInspectorGUI()
        {
            MaestroManager manager = (MaestroManager)target;

            EditorGUILayout.LabelField("Main Configuration2", EditorStyles.boldLabel);
            leftHand.objectReferenceValue = (MaestroHandV2)EditorGUILayout.ObjectField("Left Hand", manager.LeftHand, typeof(MaestroHandV2), allowSceneObjects: true);
            rightHand.objectReferenceValue = (MaestroHandV2)EditorGUILayout.ObjectField("Right Hand", manager.RightHand, typeof(MaestroHandV2), allowSceneObjects: true);
            EditorGUILayout.Space();


            if (manager.LeftHand == null && manager.RightHand == null)
            {
                EditorGUILayout.HelpBox("Hands not defined! Settings cannot yet be configured!", MessageType.Warning, wide: true);
            }
            else if (manager.LeftHand == null)
            {
                EditorGUILayout.HelpBox("Left hand not defined! Two-handed interactions will disabled!", MessageType.Warning, wide: true);
            }
            else if (manager.RightHand == null)
            {
                EditorGUILayout.HelpBox("Right hand not defined! Two-handed interactions will disabled!", MessageType.Warning, wide: true);
            }

            if (manager.LeftHand != null && manager.LeftHand.settingsOverride)
                EditorGUILayout.HelpBox("Left hand will not take these settings because override is on.", MessageType.Info, wide: true);
            if (manager.RightHand != null && manager.RightHand.settingsOverride)
                EditorGUILayout.HelpBox("Right hand will not take these settings because override is on.", MessageType.Info, wide: true);


            if (manager.LeftHand != null || manager.RightHand != null)
            {

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Default Interaction Profile", EditorStyles.boldLabel);
                interactablesOnly.boolValue = EditorGUILayout.Toggle(new GUIContent("Interactables Only", "hover text"), manager.InteractablesOnly);
                
                if (!manager.InteractablesOnly)
                {
                    defaultEffect.FindPropertyRelative("Amplitude").intValue = 
                        (byte)EditorGUILayout.IntSlider("Feedback Amplitude", 
                        manager.DefaultEffect.Amplitude, 
                        HapticEffect.FORCE_FEEDBACK_MIN_AMPLITUDE, 
                        HapticEffect.FORCE_FEEDBACK_MAX_AMPLITUDE);
                    defaultEffect.FindPropertyRelative("Vibration").intValue = 
                        (byte)EditorGUILayout.IntSlider("Vibration Effect", 
                        manager.DefaultEffect.Vibration, 
                        HapticEffect.VIBRATION_MIN_ID, 
                        HapticEffect.VIBRATION_MAX_ID);
                    EditorGUILayout.HelpBox(DRV2605Descriptions.get(manager.DefaultEffect.Vibration), MessageType.None, false);
                }

                showHandConfig = EditorGUILayout.Foldout(showHandConfig, showHandConfigTxt);
                if (showHandConfig)
                {
                    EditorGUILayout.LabelField("Hand Sizing", EditorStyles.boldLabel);
                    handSize.FindPropertyRelative("TipSize").floatValue = EditorGUILayout.FloatField("Tip Size", manager.handSize.TipSize);
                    handSize.FindPropertyRelative("MiddleSize").floatValue = EditorGUILayout.FloatField("Middle Size", manager.handSize.MiddleSize);
                    handSize.FindPropertyRelative("KnuckleSize").floatValue = EditorGUILayout.FloatField("Knuckle Size", manager.handSize.KnuckleSize);

                    EditorGUILayout.Space();
                }

                showAdvConfig = EditorGUILayout.Foldout(showAdvConfig, showAdvConfigTxt);
                if (showAdvConfig)
                {
                    EditorGUILayout.LabelField("Optional", EditorStyles.boldLabel);
                    flatnessChecker.objectReferenceValue = (FlatnessChecker)EditorGUILayout.ObjectField("Flatness Checker", manager.flatnessChecker, typeof(FlatnessChecker), allowSceneObjects: true);
                    objectLayer.intValue = EditorGUILayout.LayerField("Object Layer", manager.objectLayer);
                    EditorGUILayout.Space();

                    palmMeshWait.floatValue = EditorGUILayout.FloatField("Palm Mesh Generation Tick", manager.palmMeshWait);
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Finger Collider Reset", EditorStyles.boldLabel);
                    tooClose.floatValue = EditorGUILayout.FloatField("Too Close Threshold", manager.tooClose);
                    tooFast.floatValue = EditorGUILayout.FloatField("Too Fast Threshold", manager.tooFast);
                }
            }

            manager.cascadeProperties();
            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
