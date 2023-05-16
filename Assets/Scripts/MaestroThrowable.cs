using Leap.Unity;
using Leap.Unity.HandsModule;
using Leap.Unity.Interaction.PhysicsHands;
using Leap.Unity.Preview.InputActions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Leap.Unity.Preview.InputActions.LeapInputActionUpdater;

public class MaestroThrowable : MonoBehaviour
{
    private PhysicsProvider PhysicsProvider;
    private Rigidbody rb;
    private bool isGrasped;
    private PhysicsHand graspedHand;
    private HandModelBase handModelBase;
    private float extraThrowForce = 2f;
    private float minVelocityForThrow = 1.4f;

    /* Track velocity for last 4 frames 
      If lost tracking and velocity in the last 4 frames was enough to be considered a throw, throw the object

     */
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
    private IEnumerator CalculateVelocity()
    {
        List<Vector3> velocitySamples = new List<Vector3>();
        Vector3 averageVelocity = Vector3.zero;
        for (int i = 0; i < 8; i++)
        {
            velocitySamples.Add(rb.velocity);
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(0.04f);
        foreach (Vector3 velocity in velocitySamples)
        {
            averageVelocity += velocity;
        }
        averageVelocity = averageVelocity / velocitySamples.Count;
        if (averageVelocity.magnitude > minVelocityForThrow)
        {
            Throw(averageVelocity);
        }
    }

    private void Throw(Vector3 averageVelocity)
    {
        rb.AddForce(averageVelocity.normalized * extraThrowForce, ForceMode.Impulse);
    }
    private void CheckForThrow(Rigidbody arg1, PhysicsGraspHelper arg2)
    {

        if (isGrasped && PhysicsProvider.IsGraspingObject(rb) == false)
        {
            isGrasped = false;
            StartCoroutine(CalculateVelocity());
            graspedHand = null;
        }

        if (arg1 == rb && arg2.GraspState == PhysicsGraspHelper.State.Grasp)
        {
            isGrasped = true;
            graspedHand = PhysicsProvider.LeftHand.IsGrasping == true ? PhysicsProvider.LeftHand : PhysicsProvider.RightHand;
            handModelBase = FindObjectsOfType<HandBinder>().First(o => o.Handedness == graspedHand.Handedness);
        }

    }

    private void FixedUpdate()
    {

    }
}
