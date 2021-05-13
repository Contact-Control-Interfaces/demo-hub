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

        private bool showDebug = false;
        private string showDebugTxt = "Debug";


        public void OnEnable()
        {
            /* Effectively the inspector's OnStart */
        }

        public override void OnInspectorGUI()
        {
            MaestroManager manager = (MaestroManager)target;

            EditorGUILayout.LabelField("Main Configuration", EditorStyles.boldLabel);
            manager.LeftHand = (MaestroHandV2)EditorGUILayout.ObjectField("Left Hand", manager.LeftHand, typeof(MaestroHandV2), allowSceneObjects: true);
            manager.RightHand = (MaestroHandV2)EditorGUILayout.ObjectField("Right Hand", manager.RightHand, typeof(MaestroHandV2), allowSceneObjects: true);
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
                manager.InteractablesOnly = EditorGUILayout.Toggle(new GUIContent("Interactables Only", "hover text"), manager.InteractablesOnly);
                if (!manager.InteractablesOnly)
                {
                    HapticEffect thisDefaultEffect = manager.DefaultEffect;
                    thisDefaultEffect.Amplitude = 
                        (byte)EditorGUILayout.IntSlider("Feedback Amplitude", 
                        thisDefaultEffect.Amplitude, 
                        HapticEffect.FORCE_FEEDBACK_MIN_AMPLITUDE, 
                        HapticEffect.FORCE_FEEDBACK_MAX_AMPLITUDE);
                    thisDefaultEffect.Vibration = 
                        (byte)EditorGUILayout.IntSlider("Vibration Effect", 
                        thisDefaultEffect.Vibration, 
                        HapticEffect.VIBRATION_MIN_ID, 
                        HapticEffect.VIBRATION_MAX_ID);
                    manager.DefaultEffect = thisDefaultEffect;
                    EditorGUILayout.HelpBox(DRV2605Descriptions.get(manager.DefaultEffect.Vibration), MessageType.None, false);
                }

                showHandConfig = EditorGUILayout.Foldout(showHandConfig, showHandConfigTxt);
                if (showHandConfig)
                {
                    EditorGUILayout.LabelField("Hand Sizing", EditorStyles.boldLabel);
                    manager.TipSize = EditorGUILayout.FloatField("Tip Size", manager.TipSize);
                    manager.MiddleSize = EditorGUILayout.FloatField("Middle Size", manager.MiddleSize);
                    manager.KnuckleSize = EditorGUILayout.FloatField("Knuckle Size", manager.KnuckleSize);
                    EditorGUILayout.Space();
                }

                showAdvConfig = EditorGUILayout.Foldout(showAdvConfig, showAdvConfigTxt);
                if (showAdvConfig)
                {
                    EditorGUILayout.LabelField("Optional", EditorStyles.boldLabel);
                    manager.flatnessChecker = (FlatnessChecker)EditorGUILayout.ObjectField("Flatness Checker", manager.flatnessChecker, typeof(FlatnessChecker), allowSceneObjects: true);
                    manager.objectLayer = EditorGUILayout.LayerField("Object Layer", manager.objectLayer);
                    EditorGUILayout.Space();

                    manager.palmMeshWait = EditorGUILayout.FloatField("Palm Mesh Generation Tick", manager.palmMeshWait);
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Finger Collider Reset", EditorStyles.boldLabel);
                    manager.tooClose = EditorGUILayout.FloatField("Too Close Threshold", manager.tooClose);
                    manager.tooFast = EditorGUILayout.FloatField("Too Fast Threshold", manager.tooFast);
                }

                manager.cascadeProperties();
            }
        }
    }
}
