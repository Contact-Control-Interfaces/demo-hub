using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MaestroHand))]
public class MaestroHandEditor : Editor {

    private Texture2D handTexture, legendTexture, leftTexture;
    private int maxWidth = 225;

    private bool showMainConfig = false;
    private string showMainConfigMsg = "Hand Configuration";

    private bool showAdvConfig = false;
    private string showAdvConfigMsg = "Advanced Configuration";

    private bool showDebug = false;
    private string showDebugMsg = "Debug";

    public void OnEnable()
    {
        // Load images for editor
        handTexture = EditorGUIUtility.Load("maestro_handr.png") as Texture2D;
        //handTexture.Resize(maxWidth, (int)(((float)handTexture.height / handTexture.width) * maxWidth));
        legendTexture = EditorGUIUtility.Load("maestro_legend.png") as Texture2D;

        leftTexture = FlipTexture(handTexture);
    }

    Texture2D FlipTexture(Texture2D original)
    {
        Texture2D flipped = new Texture2D(original.width, original.height);

        int w = original.width;
        int h = original.height;
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                flipped.SetPixel(w - i - 1, j, original.GetPixel(i, j));
            }
        }
        flipped.Apply();
        return flipped;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        MaestroHand maestroHand = (MaestroHand)target;

        // Draw hand / legend images
        GUIStyle style = new GUIStyle("Label");
        style.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.BeginVertical(style);
        GUILayout.Label(maestroHand.whichHand == WhichHand.RightHand ? handTexture : leftTexture, style,
            GUILayout.MaxWidth(maxWidth), GUILayout.ExpandWidth(true), GUILayout.MaxHeight(maxWidth));
        GUILayout.Label(legendTexture, style,
            GUILayout.MaxWidth(maxWidth), GUILayout.ExpandWidth(true), GUILayout.MaxHeight(40));
        EditorGUILayout.EndVertical();

        // Basic Configuration
        maestroHand.whichHand = (WhichHand)EditorGUILayout.EnumPopup("Which Hand", maestroHand.whichHand);
        maestroHand.otherHand = EditorGUILayout.ObjectField("Other Hand", maestroHand.otherHand, typeof(MaestroHand), allowSceneObjects: true) as MaestroHand;
        if (!maestroHand.otherHand)
            EditorGUILayout.HelpBox("Two-handed grabs won't be possible if Other Hand is not set!", MessageType.Warning, true);

        // Expose bools
        EditorGUILayout.Toggle("One-hand Grabbing", maestroHand.grabbing);
        EditorGUILayout.Toggle("Two-hand Grabbing", maestroHand.twoHandGrabbing);
        EditorGUILayout.Space();

        // Haptic defaults
        maestroHand.DefaultPull = (byte)EditorGUILayout.IntSlider("Default Pull", maestroHand.DefaultPull, 0, 255);
        maestroHand.DefaultVib = (byte)EditorGUILayout.IntSlider("Default Vibration", maestroHand.DefaultVib, 0, 128);
        EditorGUILayout.HelpBox(DRV2605Descriptions.get(maestroHand.DefaultVib), MessageType.None, false);
        EditorGUILayout.Space();

        showMainConfig = EditorGUILayout.Foldout(showMainConfig, showMainConfigMsg);
        if (showMainConfig)
        {
            // Thumb
            maestroHand.ThumbTip = EditorGUILayout.ObjectField("Thumb Tip", maestroHand.ThumbTip, typeof(GameObject), allowSceneObjects: true) as GameObject;
            maestroHand.ThumbMiddle = EditorGUILayout.ObjectField("Thumb Middle", maestroHand.ThumbMiddle, typeof(GameObject), allowSceneObjects: true) as GameObject;
            maestroHand.ThumbMeta = EditorGUILayout.ObjectField("Thumb Meta", maestroHand.ThumbMeta, typeof(GameObject), allowSceneObjects: true) as GameObject;

            // Index
            maestroHand.IndexTip = EditorGUILayout.ObjectField("Index Tip", maestroHand.IndexTip, typeof(GameObject), allowSceneObjects: true) as GameObject;
            maestroHand.IndexMiddle = EditorGUILayout.ObjectField("Index Middle", maestroHand.IndexMiddle, typeof(GameObject), allowSceneObjects: true) as GameObject;

            // Middle
            maestroHand.MiddleTip = EditorGUILayout.ObjectField("Middle Tip", maestroHand.MiddleTip, typeof(GameObject), allowSceneObjects: true) as GameObject;
            maestroHand.MiddleMiddle = EditorGUILayout.ObjectField("Middle Middle", maestroHand.MiddleMiddle, typeof(GameObject), allowSceneObjects: true) as GameObject;

            // Ring
            maestroHand.RingTip = EditorGUILayout.ObjectField("Ring Tip", maestroHand.RingTip, typeof(GameObject), allowSceneObjects: true) as GameObject;
            maestroHand.RingMiddle = EditorGUILayout.ObjectField("Ring Middle", maestroHand.RingMiddle, typeof(GameObject), allowSceneObjects: true) as GameObject;

            // Little
            maestroHand.LittleTip = EditorGUILayout.ObjectField("Little Tip", maestroHand.LittleTip, typeof(GameObject), allowSceneObjects: true) as GameObject;
            maestroHand.LittleMiddle = EditorGUILayout.ObjectField("Little Middle", maestroHand.LittleMiddle, typeof(GameObject), allowSceneObjects: true) as GameObject;

            // Palm
            maestroHand.Palm = EditorGUILayout.ObjectField("Palm", maestroHand.Palm, typeof(GameObject), allowSceneObjects: true) as GameObject;
            EditorGUILayout.Space();

            maestroHand.fingertipRadius = EditorGUILayout.FloatField("Fingertip Radius", maestroHand.fingertipRadius);
            maestroHand.palmSize = EditorGUILayout.Vector3Field("Palm Size", maestroHand.palmSize);
            maestroHand.palmOffset = EditorGUILayout.Vector3Field("Palm Offset", maestroHand.palmOffset);
            EditorGUILayout.Space();
        }

        // Show error if not all hand locations are set.
        bool allSet = maestroHand.ThumbTip && maestroHand.ThumbMiddle && maestroHand.ThumbMeta
            && maestroHand.IndexTip && maestroHand.IndexMiddle
            && maestroHand.MiddleTip && maestroHand.MiddleMiddle
            && maestroHand.RingTip && maestroHand.RingMiddle
            && maestroHand.LittleTip && maestroHand.LittleMiddle
            && maestroHand.Palm;

        if (!allSet)
            EditorGUILayout.HelpBox("Hand is not fully configured!", MessageType.Error, true);

        // Show advanced configuration
        showAdvConfig = EditorGUILayout.Foldout(showAdvConfig, showAdvConfigMsg);
        if (showAdvConfig)
        {
            //Flatness Checker
            maestroHand.flatnessChecker = EditorGUILayout.ObjectField("Flatness Checker", maestroHand.flatnessChecker, typeof(FlatnessChecker), allowSceneObjects: true) as FlatnessChecker;

            // Collider reset variables
            maestroHand.tooClose = EditorGUILayout.FloatField("Reset Distance", maestroHand.tooClose);
            maestroHand.tooFast = EditorGUILayout.FloatField("Reset Speed", maestroHand.tooFast);
            EditorGUILayout.Space();

            // Object release ratios
            maestroHand.releaseRatio = EditorGUILayout.FloatField("Release Ratio", maestroHand.releaseRatio);
            maestroHand.ToolRatio = EditorGUILayout.FloatField("Tool Release Ratio", maestroHand.ToolRatio);
            EditorGUILayout.Space();

            // Layers
            // TODO change to LayerField
            maestroHand.objectLayer = EditorGUILayout.TextField("Grabbed Object Layer", maestroHand.objectLayer);
            maestroHand.ignoreLayer = EditorGUILayout.TextField("Ignore While Gripped Layer", maestroHand.ignoreLayer);
            EditorGUILayout.Space();

            // Others
            maestroHand.scaler = EditorGUILayout.FloatField("Anchor Movement Scalar", maestroHand.scaler);
            maestroHand.fingertipPhysicMaterial = EditorGUILayout.ObjectField("Physics Material", maestroHand.fingertipPhysicMaterial, typeof(PhysicMaterial), allowSceneObjects: true) as PhysicMaterial;

            EditorGUILayout.Space();
        }

        // Show debug stuff
        showDebug = EditorGUILayout.Foldout(showDebug, showDebugMsg);
        if (showDebug)
        {
            // Bools
            maestroHand.showColliders = EditorGUILayout.Toggle("Show Colliders", maestroHand.showColliders);
            EditorGUILayout.Space();

            // Time variables
            EditorGUILayout.LabelField("t Since One-hand", string.Format("{0:f} s", maestroHand.timeSinceGrabbing));
            EditorGUILayout.LabelField("t Since Release", string.Format("{0:f} s", maestroHand.timeSinceRelease));
            EditorGUILayout.LabelField("t Since Two-hand", string.Format("{0:f} s", maestroHand.timeSinceTwoHandGrabbing));
            EditorGUILayout.Space();

            // Current Grab Info
            EditorGUILayout.LabelField("First Index", maestroHand.f1.ToString());
            EditorGUILayout.LabelField("Second Index", maestroHand.f2.ToString());
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("First Distance", maestroHand.dist1.ToString());
            EditorGUILayout.LabelField("Second Distance", maestroHand.dist2.ToString());
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("First Ratio", maestroHand.ratio1.ToString());
            EditorGUILayout.LabelField("Second Ratio", maestroHand.ratio2.ToString());

            EditorGUILayout.Space();
        }
    }
}
