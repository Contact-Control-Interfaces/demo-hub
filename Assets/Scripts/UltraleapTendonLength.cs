using Leap.Unity;
using Maestro;
using System;
using UnityEngine;

public class UltraleapTendonLength : TendonLength
{
    private HandModelBase provider;
    private MaestroHand hand;

    private Leap.Hand leapHand;
    private Leap.Finger leapFinger => leapHand.Fingers[LeapFingerIndex];

    [Range(0f, 90f)]
    public float Rotation;
    public int LeapFingerIndex;

    public override float Radius => leapFinger.Width;

    public override float[] JointRotations
    {
        get
        {
            Leap.Bone[] bones = leapFinger.bones;

            float[] result = new float[bones.Length];

            for (int i = 0; i < bones.Length; i++) {
                result[i] = Rotation * Mathf.Deg2Rad;
            }

            return result;
        }
    }

    protected override float BoneLength
    {
        get
        {
            float result = 0f;

            if (leapHand == null)
                return 0f;

            // just using the index for now
            foreach (Leap.Bone bone in leapFinger.bones) {
                result += bone.Length;
            }

            return result;
        }
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
