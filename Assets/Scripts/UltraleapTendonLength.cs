using Leap.Unity;
using Maestro;
using System;
using System.Linq;
using UnityEngine;

public class UltraleapTendonLength : TendonLength
{
    private HandModelBase provider;

    private Leap.Hand leapHand;
    private Leap.Finger leapFinger => leapHand?.Fingers[LeapFingerIndex];

    public int LeapFingerIndex;

    public Vector3 FingerDirectionInLocalSpace = Vector3.right;

    public override float Radius => leapFinger == null ? 0f : leapFinger.Width;

    public float MaxRotationDegrees = 90f;

    public override float[] JointRotations
    {
        get
        {
            if (leapHand == null)
                return new float[] { 0f };

            // 1.0 is fully curled, 0 is not at all
            float curl = leapHand.GetFingerStrength(LeapFingerIndex);

            // assume all three digits are curled the same amount for now
            return Enumerable.Repeat(curl * MaxRotationDegrees * Mathf.Deg2Rad, leapFinger.bones.Length).ToArray();
        }
    }

    protected override float BoneLength
    {
        get
        {
            if (leapFinger == null)
                return 0f;

            // They provide a bone length already so that's nice
            return leapFinger.bones.Aggregate(0f, (acc, bone) => acc + bone.Length);
        }
    }

    private void Awake()
    {
        if (provider == null)
            provider = this.GetComponentInParent<HandModelBase>();

        if (provider == null) {
            Debug.LogError($"No LeapProvider found on object [{this.gameObject.name}]! Disabling...");
            this.enabled = false;
        } else {
            leapHand = provider.GetLeapHand();
        }
    }

    private void Update()
    {
        leapHand = provider.GetLeapHand();
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
