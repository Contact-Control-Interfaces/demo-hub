using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(MaestroInteractable))]
public class MaestroInteractableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MaestroInteractable mi = target as MaestroInteractable;
        EditorGUILayout.HelpBox(DRV2605Descriptions.get(mi.VibrationEffect), MessageType.None, false);
    }
}
