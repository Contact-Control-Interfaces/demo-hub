using Maestro.Vibration;
using Maestro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CustomHapticSender : MonoBehaviour
{
    public MaestroInteractable interactable;
    public HapticEffect WristHaptics = new HapticEffect { Amplitude = 200, Vibration = { Effect = new None() } };
    public MaestroHand wristHand;

    public void Start()
    {
        wristHand = FindObjectsOfType<MaestroHand>().Where(hand => hand.whichHand == WhichHand.LeftHand)
            .FirstOrDefault();
    }
    public void StartHaptics()
    {
        interactable.SetHapticOverride(WristHaptics);
        interactable.persistenceDuration = 1f;
        wristHand.ApplyAllGlobalInteractable(interactable);
    }

    public void StopHaptics()
    {
        interactable.SetHapticOverride(new HapticEffect() { Amplitude = 0, Vibration = { Effect = new None() } });
        interactable.persistenceDuration = .005f;
        wristHand.ApplyAllGlobalInteractable(interactable);
    }
}
