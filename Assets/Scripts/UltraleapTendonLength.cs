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

    public override float Radius => leapFinger == null ? 0f : leapFinger.Width;

    public float MaxRotationDegrees = 90f;

    public override float[] JointRotations
    {
        get
        {
            if (leapHand == null)
                return new float[] { 0f };

            // Get each finger bone direction in world space
            Vector3[] directions = new Vector3[leapFinger.bones.Length];
            for (int i = 0; i < directions.Length; i++) {
                directions[i] = leapFinger.bones[i].Direction;
            }

            // Calculate angle between each pair of bones
            float[] angles = new float[directions.Length - 1]; // -1 since we are calculating differences of pairs
            for (int i = 0; i < angles.Length; i++) {
                angles[i] = Mathf.Deg2Rad * Vector3.Angle(directions[i], directions[i + 1]);
            }

            return angles;
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
