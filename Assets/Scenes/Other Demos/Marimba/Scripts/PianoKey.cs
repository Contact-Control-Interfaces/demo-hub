using Maestro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoKey : MonoBehaviour
{
    private MaestroInteractable interactable;

    public byte UnpressedAmplitude;
    public byte PressAmplitude;

    // Start is called before the first frame update
    void Start()
    {
        interactable = this.GetComponent<MaestroInteractable>();

        UnPress();
    }

    public void Press()
    {
        interactable.StayHaptics.Amplitude = PressAmplitude;
    }

    public void UnPress()
    {
        interactable.StayHaptics.Amplitude = UnpressedAmplitude;
    }
}
