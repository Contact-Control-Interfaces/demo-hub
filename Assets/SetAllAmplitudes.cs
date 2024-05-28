using Maestro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SetAllAmplitudes : MonoBehaviour
{
    public GameObject[] ToSearch;
    public MaestroInteractable[] Except;

    public MaestroInteractable[] Found;

    void Start()
    {
        var include = ToSearch.SelectMany(x => x.GetComponentsInChildren<MaestroInteractable>()).Distinct();
        Found = include.Except(Except).ToArray();
    }

    public void UpdateAllAmplitudes(float amplitude)
    {
        foreach (MaestroInteractable mi in Found) {
            byte toSet = (byte)(amplitude * 255);
            mi.stayHaptics.Amplitude = toSet;
            mi.startHaptics.Amplitude = toSet;
            mi.exitHaptics.Amplitude = toSet;
        }
    }
}
