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

    public Vector3 FingerDirectionInLocalSpace = Vector3.right;

    public override float Radius => leapFinger.Width;

    public override float[] JointRotations
    {
        get
        {
            Leap.Bone[] bones = leapFinger.bones;
            Vector3[] directions = new Vector3[bones.Length + 1]; // +1 to store the base hand direction too

            // Make an array of which direction each joint in pointing
            directions[0] = this.transform.TransformDirection(FingerDirectionInLocalSpace);
            for (int i = 0; i < bones.Length; i++) {
                directions[i + 1] = bones[i].Direction;
            }

            // Each joint's rotation is just the angle between this bone and the previous
            float[] result = new float[bones.Length];
            for (int i = 0; i < result.Length; i++) {
                result[i] = Mathf.Deg2Rad * Vector3.Angle(directions[i + 1], directions[i]);
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
