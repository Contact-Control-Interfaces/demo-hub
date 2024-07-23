using Leap.Unity;
using Leap.Unity.PhysicalHands;
using Maestro;
using System;
using System.Linq;
using UnityEngine;

public class UltraleapTendonLength : TendonLength
{
    public int LeapFingerIndex;
    public float MaxRotationDegrees = 90f;

    private HandModelBase provider;
    private PhysicalHandsManager physicsHandManager;

    private Leap.Hand physicsLeapHand;
    private Leap.Hand realLeapHand;

    private Leap.Finger leapFinger => GetFinger(physicsLeapHand);

    public override float Radius => leapFinger != null ? leapFinger.Width : 0f;

    public override float[] VirtualJointRotations => GetJointRotations(physicsLeapHand);

    public override float[] RealJointRotations => GetJointRotations(realLeapHand);

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

    private Leap.Finger GetFinger(Leap.Hand hand) => hand?.Fingers[LeapFingerIndex];

    private float[] GetJointRotations(Leap.Hand hand)
    {
        if (hand == null)
            return new float[] { 0f };

        Leap.Finger leapFinger = GetFinger(hand);

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

    private void Awake()
    {
        if (provider == null)
            provider = this.GetComponentInParent<HandModelBase>();

        if (provider != null && physicsHandManager == null)
            physicsHandManager = FindObjectOfType<PhysicalHandsManager>();

        if (provider == null) {
            Debug.LogError($"No LeapProvider found on object [{this.gameObject.name}]! Disabling...");
            this.enabled = false;
        } else {
            UpdateHandData();
        }
    }

    private void Update()
    {
        UpdateHandData();
    }

    protected void UpdateHandData()
    {
        physicsLeapHand = physicsHandManager.GetHand(provider.Handedness);
        realLeapHand = provider.GetLeapHand();
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
