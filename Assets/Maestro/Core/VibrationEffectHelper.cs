using Maestro;
using Maestro.Vibration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class VibrationEffectHelper : MonoBehaviour
{
    public int anInt;

    [SerializeReference]
    public VibrationEffect temp = new DoubleSharpTick(TickDuration.Long, NarrowThreeOptions._60);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[CustomEditor(typeof(VibrationEffectHelper))]
public class VibrationEffectHelperEditor : Editor
{
    private SerializedProperty temp;

    public void OnEnable()
    {
        /* Effectively the inspector's OnStart */
        temp = this.serializedObject.FindProperty("temp");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
