using Maestro;
using Maestro.Vibration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piano : MonoBehaviour
{
    public GameObject[] whiteKeys;
    public GameObject[] blackKeys;

    public bool BreakRandomKey = false;

    void Start()
    {
        if (BreakRandomKey)
            BreakRandomNote();
    }

    private void BreakRandomNote()
    {
        if (whiteKeys == null || whiteKeys.Length == 0)
            return;
            
        int index = Random.Range(0, whiteKeys.Length - 1);

        GameObject toBreak = whiteKeys[index];
        toBreak.name = "Broken " + toBreak.name;

        var source = toBreak.GetComponent<AudioSource>();
        source.clip = null;

        var interactable = toBreak.GetComponent<MaestroInteractable>();
        HapticEffect nothing = new HapticEffect() { Amplitude = 0, Vibration = MaestroHapticSingletons.None };
        interactable.startHaptics = nothing;
        interactable.stayHaptics = nothing;
    }
}
