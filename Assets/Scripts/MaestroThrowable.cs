using Leap.Unity.Interaction.PhysicsHands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MaestroThrowable : MonoBehaviour
{
    private PhysicsProvider PhysicsProvider;
    private Rigidbody rb;
    private bool isGrasped;
    private PhysicsHand graspedHand;
    private float extraThrowForce = 9f;
    private float minVelocityForThrow = 1.4f;

    private void Start()
    {
        PhysicsProvider = FindObjectOfType<PhysicsProvider>();
        rb = GetComponent<Rigidbody>();
        PhysicsProvider.OnObjectStateChange += CheckForThrow;
    }

    private void OnDestroy()
    {
        PhysicsProvider.OnObjectStateChange -= CheckForThrow;
    }
    private IEnumerator ThrowCoroutine()
    {
        List<Vector3> velocitySamples = new List<Vector3>();
        for (int i=0; i < 8; i++)
        {
            velocitySamples.Add(rb.velocity);
            yield return null;
        }
        yield return new WaitForSeconds(0.04f);
        Vector3 averageVelocity = new Vector3();
        foreach(Vector3 velocity in velocitySamples)
        {
            averageVelocity += velocity;
        }
        averageVelocity = averageVelocity / velocitySamples.Count;
        if (averageVelocity.magnitude > minVelocityForThrow)
        {
            rb.AddForce(averageVelocity.normalized * 2f, ForceMode.Impulse);
        }
    }
    private void CheckForThrow(Rigidbody arg1, PhysicsGraspHelper arg2)
    {
        if (isGrasped && PhysicsProvider.IsGraspingObject(rb) == false)
        {
            isGrasped = false;
            StartCoroutine(ThrowCoroutine());
            graspedHand = null;
        }

        if (arg1 == rb && arg2.GraspState == PhysicsGraspHelper.State.Grasp)
        {
            isGrasped = true;
            graspedHand = PhysicsProvider.LeftHand.IsGrasping == true ? PhysicsProvider.LeftHand : PhysicsProvider.RightHand;
        }

    }
}
