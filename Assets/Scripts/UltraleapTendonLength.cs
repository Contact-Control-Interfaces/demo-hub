using Leap.Unity;
using Maestro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltraleapTendonLength : TendonLength
{
    private LeapProvider provider;
    private MaestroHand hand;

    private List<Leap.Finger> Fingers;

    protected override float BoneLength()
    {
        throw new System.NotImplementedException();
    }

    private void Awake()
    {
        if (hand == null)
            hand = this.GetComponentInParent<MaestroHand>();

        if (provider == null)
            provider = this.GetComponentInChildren<LeapProvider>();

        if (provider == null) {
            Debug.LogError($"No LeapProvider found on object [{this.gameObject.name}]! Disabling...");
            this.enabled = false;
        } else if (hand == null) {
            Debug.LogError($"No MaestroHand found for object [{this.gameObject.name}]! Disabling...");
            this.enabled = false;
        } else {
            var leapHand = provider.GetHand(HandednessToChirality(hand.whichHand));
            Fingers = leapHand.Fingers;
        }
    }

    private WhichHand ChiralityToHandedness(Chirality chirality)
    {
        return chirality switch {
            Chirality.Left => WhichHand.LeftHand,
            Chirality.Right => WhichHand.RightHand,
            _ => throw new ArgumentOutOfRangeException($"Unknown chirality {chirality}!")
        };
    }

    private Chirality HandednessToChirality(WhichHand handedness)
    {
        return handedness switch
        {
            WhichHand.LeftHand => Chirality.Left,
            WhichHand.RightHand => Chirality.Right,
            _ => throw new ArgumentOutOfRangeException($"Unknown handedness {handedness}")
        };
    }
}
