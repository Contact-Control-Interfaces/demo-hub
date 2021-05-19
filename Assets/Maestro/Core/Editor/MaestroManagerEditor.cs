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

        SerializedProperty leftHand;
        SerializedProperty rightHand;
        
        SerializedProperty interactablesOnly;

        SerializedProperty tipSize;
        SerializedProperty middleSize;
        SerializedProperty knuckleSize;

        SerializedProperty flatnessChecker;
        SerializedProperty objectLayer;
        SerializedProperty palmMeshWait;
        SerializedProperty tooClose;
        SerializedProperty tooFast;

        public void OnEnable()
        {
            leftHand = this.serializedObject.FindProperty("LeftHand");
            rightHand = this.serializedObject.FindProperty("RightHand");
            
            interactablesOnly = this.serializedObject.FindProperty("InteractablesOnly");

            tipSize = this.serializedObject.FindProperty("TipSize");
            middleSize = this.serializedObject.FindProperty("MiddleSize");
            knuckleSize = this.serializedObject.FindProperty("KnuckleSize");

            flatnessChecker = this.serializedObject.FindProperty("flatnessChecker");
            objectLayer = this.serializedObject.FindProperty("objectLayer");
            palmMeshWait = this.serializedObject.FindProperty("palmMeshWait");
            tooClose = this.serializedObject.FindProperty("tooClose");
            tooFast = this.serializedObject.FindProperty("tooFast");
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
                    byte Amplitude = manager.DefaultEffect.Amplitude;
                    byte Vibration = manager.DefaultEffect.Vibration;
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
                    EditorGUILayout.HelpBox(DRV2605Descriptions.get(manager.DefaultEffect.Vibration), MessageType.None, false);

                    var childEnum = this.serializedObject.FindProperty("DefaultEffect").GetEnumerator();
                    while (childEnum.MoveNext())
                    {
                        SerializedProperty current = childEnum.Current as SerializedProperty;
                        if (current.name.Equals("Amplitude"))
                            current.intValue = Amplitude;
                        else if (current.name.Equals("Vibration"))
                            current.intValue = Vibration;
                    }
                   
                }

                showHandConfig = EditorGUILayout.Foldout(showHandConfig, showHandConfigTxt);
                if (showHandConfig)
                {
                    EditorGUILayout.LabelField("Hand Sizing", EditorStyles.boldLabel);
                    tipSize.floatValue = EditorGUILayout.FloatField("Tip Size", manager.TipSize);
                    middleSize.floatValue = EditorGUILayout.FloatField("Middle Size", manager.MiddleSize);
                    knuckleSize.floatValue = EditorGUILayout.FloatField("Knuckle Size", manager.KnuckleSize);
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
