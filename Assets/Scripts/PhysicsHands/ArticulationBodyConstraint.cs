using Leap;
using Leap.Unity;
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
    public float linearDamping;

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
        linearDamping = body.linearDamping;
    }
}
public class ArticulationBodyConstraint : MonoBehaviour
{
    private PhysicsHand physicsHand;
    private Dictionary<ArticulationBody, ArticulationBodyData> initialBodyVelocities = new Dictionary<ArticulationBody, ArticulationBodyData>();
    private float initialMaximumPalmVelocity;
    private float initialMaxmimumFingerVelocity;
    private PhysicsHand.Hand hand;
    private Transform rig;
    private Vector3 originalPosition;
    private void Awake()
    {
        physicsHand = GetComponent<PhysicsHand>();
        hand = physicsHand.GetPhysicsHand();
        rig = FindObjectOfType<MaestroManager>().transform;
        physicsHand.OnUpdatePhysics += ConstrainWholeHand;
        physicsHand.OnBeginPhysics += SetupJoints;
    }

    private void SetupJoints()
    {
        initialBodyVelocities[hand.palmBone.ArticulationBody] = new ArticulationBodyData(hand.palmBone.ArticulationBody);
        initialMaximumPalmVelocity = hand.maximumPalmVelocity;
        initialMaxmimumFingerVelocity = hand.maximumFingerVelocity;
        foreach (PhysicsBone bone in hand.jointBones)
        {
            initialBodyVelocities[bone.ArticulationBody] = new ArticulationBodyData(bone.ArticulationBody);
            if(bone.TryGetComponent(out CapsuleCollider collider))
            {
               CapsuleCollider trigger = bone.gameObject.AddComponent<CapsuleCollider>();
                trigger.center = collider.center; 
                trigger.radius = collider.radius / 2f;
                trigger.height = collider.height;
                trigger.direction = collider.direction;
                trigger.isTrigger = true;
            }
        }
    }
    private void OnDestroy()
    {
        physicsHand.OnUpdatePhysics -= ConstrainWholeHand;
    }

    private void ConstrainWholeHand()
    {
        ConstrainPalm();
    }

    private void ConstrainPalm()
    {
        PhysicsBone palmBone = hand.palmBone;
        PhysicsBoneContactInfo contactInfo = palmBone.GetComponent<PhysicsBoneContactInfo>();
        bool palmIsContacting = palmBone.ContactingObjects.Where(o => o.GetComponent<FingerCollider>() == null).Any();
        if (!physicsHand.IsGrasping && palmBone.IsContacting && palmIsContacting)
        {
            palmBone.ArticulationBody.maxDepenetrationVelocity = 1000f;
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
}
