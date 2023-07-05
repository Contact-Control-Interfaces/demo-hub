using Leap.Unity;
using Maestro;
using System;
using UnityEngine;

public class UltraleapTendonLength : TendonLength
{
    private HandModelBase provider;
    private MaestroHand hand;

    private Leap.Hand leapHand;

    public override float BoneLength()
    {
        float result = 0f;

        if (leapHand == null)
            return 0f;

        // just using the index for now
        foreach (Leap.Bone bone in leapHand.Fingers[1].bones) {
            result += bone.Length;
        }

        return result;
    }

    private void Awake()
    {
        if (hand == null)
            hand = this.GetComponentInParent<MaestroHand>();

        if (provider == null)
            provider = this.GetComponentInParent<HandModelBase>();

        if (provider == null) {
            Debug.LogError($"No LeapProvider found on object [{this.gameObject.name}]! Disabling...");
            this.enabled = false;
        } else if (hand == null) {
            Debug.LogError($"No MaestroHand found for object [{this.gameObject.name}]! Disabling...");
            this.enabled = false;
        } else {
            leapHand = provider.GetLeapHand();
        }
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
