using Leap;
using Leap.Unity.Interaction.PhysicsHands;
using Maestro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class ArticulationBodyData
{
    public float maxAngularVelocity;
    public float maxDepenetrationVelocity;
    public float maxJointVelocity;
    public float maxLinearVelocity;
    public ArticulationDrive xDrive;
    public float angularDamping;
    public float mass;
    public float jointFriction;

    public ArticulationBodyData(ArticulationBody body)
    {
        maxAngularVelocity = body.maxAngularVelocity;
        maxDepenetrationVelocity = body.maxDepenetrationVelocity;
        maxJointVelocity = body.maxJointVelocity;
        maxLinearVelocity = body.maxJointVelocity;
        xDrive = body.xDrive;
        angularDamping = body.angularDamping;
        mass = body.mass;
        jointFriction = body.jointFriction;
    }
}
public class ArticulationBodyConstraint : MonoBehaviour
{
    private PhysicsHand physicsHand;
    private Dictionary<ArticulationBody, ArticulationBodyData> initialBodyVelocities = new Dictionary<ArticulationBody, ArticulationBodyData>();
    private float initialMaximumPalmVelocity;
    private float initialMaxmimumFingerVelocity;
    private PhysicsHand.Hand hand;
    private void Awake()
    {
        physicsHand = GetComponent<PhysicsHand>();
        hand = physicsHand.GetPhysicsHand();
        foreach (PhysicsBone bone in hand.jointBones)
        {
            initialBodyVelocities.Add(bone.ArticulationBody, new ArticulationBodyData(bone.ArticulationBody));
            bone.ArticulationBody.maxAngularVelocity = 0.1f;
        }
        initialBodyVelocities.Add(hand.palmBone.ArticulationBody, new ArticulationBodyData(hand.palmBone.ArticulationBody));
        initialMaximumPalmVelocity = hand.maximumPalmVelocity;
        initialMaxmimumFingerVelocity = hand.maximumFingerVelocity;
    }
    private bool IsAnyBoneContacting()
    {
        foreach (PhysicsBone bone in hand.jointBones)
        {
            List<Collider> correctColliders = bone.ContactingObjects.Where(o => o.GetComponent<FingerCollider>() == null && o.GetComponent<Collider>() != null).Select(c => c.GetComponent<Collider>()).ToList();
            if (bone.IsContacting && correctColliders.Count > 0)
            {
                return true;
            }
        }
            return false;
    }

    private bool IsPalmContacting()
    {
        PhysicsBone palmBone = hand.palmBone;
        List<Rigidbody> palmRigidbodies = palmBone.ContactingObjects.Where(o => o.GetComponent<FingerCollider>() == null).ToList();
        return palmBone.IsContacting && palmRigidbodies.Count > 0;
    }

    private void Update()
    {
        ConstrainWholeHand();
    }

    private void ConstrainWholeHand()
    {
        if (IsAnyBoneContacting() || IsPalmContacting() && !physicsHand.IsGrasping)
        {
            hand.maximumPalmVelocity = 0.05f;
            hand.maximumPalmAngularVelocity = 2f;
        }
        else
        {
            hand.maximumPalmVelocity = initialMaximumPalmVelocity;
            hand.maximumFingerVelocity = initialMaxmimumFingerVelocity;
            hand.maximumPalmAngularVelocity = 6000f;
        }
        foreach (PhysicsBone bone in hand.jointBones)
        {
            ConstrainFinger(bone);
        }
        ConstrainPalm();
    }

    private void ConstrainPalm()
    {
        PhysicsBone palmBone = hand.palmBone;
        List<Rigidbody> palmRigidbodies = palmBone.ContactingObjects.Where(o => o.GetComponent<FingerCollider>() == null).ToList();
        if (palmBone.IsContacting && palmRigidbodies.Count > 0)
        {
            hand.maximumPalmVelocity = 0.05f;
            palmBone.ArticulationBody.jointFriction = 10000f;
            palmBone.ArticulationBody.velocity = Vector3.zero;
            palmBone.ArticulationBody.angularVelocity = Vector3.zero;
            palmBone.ArticulationBody.maxAngularVelocity = 0.0f;
            palmBone.ArticulationBody.maxDepenetrationVelocity = 0.0f;
            palmBone.ArticulationBody.maxJointVelocity = 0.0f;
            palmBone.ArticulationBody.maxLinearVelocity = 0.0f;
            palmBone.ArticulationBody.angularDamping = 500f;
        }
        else
        {
            hand.maximumPalmVelocity = initialMaximumPalmVelocity;
            hand.maximumPalmAngularVelocity = 6000f;
            ArticulationBodyData data = initialBodyVelocities[palmBone.ArticulationBody];
            palmBone.ArticulationBody.maxAngularVelocity = data.maxAngularVelocity;
            palmBone.ArticulationBody.maxDepenetrationVelocity = data.maxDepenetrationVelocity;
            palmBone.ArticulationBody.maxJointVelocity = data.maxJointVelocity;
            palmBone.ArticulationBody.maxLinearVelocity = data.maxLinearVelocity;
            palmBone.ArticulationBody.xDrive = data.xDrive;
            palmBone.ArticulationBody.angularDamping = data.angularDamping;
            palmBone.ArticulationBody.mass = data.mass;
        }
    }

    private void ConstrainFinger(PhysicsBone bone)
    {
        List<Collider> correctColliders = bone.ContactingObjects.Where(o => o.GetComponent<FingerCollider>() == null && o.GetComponent<Collider>() != null).Select(c => c.GetComponent<Collider>()).ToList();
        if (bone.IsContacting && correctColliders.Count > 0)
        {
            bone.ArticulationBody.maxAngularVelocity = 0.01f;
            bone.ArticulationBody.maxDepenetrationVelocity = 0.01f;
            bone.ArticulationBody.maxJointVelocity = 0.01f;
            bone.ArticulationBody.maxLinearVelocity = 0.01f;
            bone.ArticulationBody.jointFriction = 1000f;
            bone.ArticulationBody.xDrive = new ArticulationDrive
            {
                forceLimit = 0.01f,
                stiffness = 10000f,
                damping = bone.ArticulationBody.xDrive.damping,
                lowerLimit = -10f,
                upperLimit = 10f,
                target = bone.ArticulationBody.xDrive.target,
                targetVelocity = bone.ArticulationBody.xDrive.targetVelocity
            };
            bone.ArticulationBody.velocity = Vector3.zero;
            bone.ArticulationBody.angularVelocity = Vector3.zero;
        }
        else
        {
            ArticulationBodyData data = initialBodyVelocities[bone.ArticulationBody];
            bone.ArticulationBody.maxAngularVelocity = data.maxAngularVelocity;
            bone.ArticulationBody.maxDepenetrationVelocity = data.maxDepenetrationVelocity;
            bone.ArticulationBody.maxJointVelocity = data.maxJointVelocity;
            bone.ArticulationBody.maxLinearVelocity = data.maxLinearVelocity;
            bone.ArticulationBody.xDrive = data.xDrive;
            bone.ArticulationBody.mass = data.mass;
            bone.ArticulationBody.jointFriction = data.jointFriction;
        }
    }

}
