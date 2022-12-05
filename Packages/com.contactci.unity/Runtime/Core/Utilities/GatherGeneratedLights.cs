using Maestro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class GatherGeneratedLights : MonoBehaviour
{
    public RotarySwitchBehavior rotarySwitch;

    public ScriptableLight[] lights;

    public string[] labels;

    private bool init = false;

    private void Awake()
    {
        init = false;
    }

    private void Init()
    {
        if (rotarySwitch == null)
            rotarySwitch = this.GetComponentInChildren<RotarySwitchBehavior>();

        // Get all labels in order
        labels = rotarySwitch.positions.Select(x => x.label).ToArray();

        // Get all lights
        var allLights = this.transform.GetComponentsInChildren<ScriptableLight>(true);

        // Exclude our template marker
        List<ScriptableLight> toExclude = new List<ScriptableLight>();
        if (rotarySwitch.Marker != null) {
            toExclude.Add(rotarySwitch.Marker.GetComponentInChildren<ScriptableLight>());
        }
        var generatedLights = allLights.Except(toExclude);

        lights = generatedLights.ToArray();

        init = true;
    }

    public void ToggleLight(int index)
    {
        for (int i = 0; i < lights.Length; i++) {
            lights[i].SetLight(i == index);
        }
    }

    public void ToggleLight(RotaryPosition position)
    {
        if (!init) Init();

        int index = Array.IndexOf(labels, position.label);
        ToggleLight(index);
    }
}
