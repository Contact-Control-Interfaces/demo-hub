using Leap.Unity;
using Leap.Unity.HandsModule;
using Leap.Unity.Interaction.PhysicsHands;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MaestroThrowable : MonoBehaviour
{
    private PhysicsProvider PhysicsProvider;
    private Rigidbody rb;
    private PhysicsHand graspedHand;
    private Transform palmTransform;
    private HandModelBase handModelBase;
    private bool isGrasped;
    private bool isLocatingHand;
    private float extraThrowForce = 2f;
    private float minVelocityForThrow = 1.4f;

    private void Start()
    {
        PhysicsProvider = FindObjectOfType<PhysicsProvider>();
        rb = GetComponent<Rigidbody>();
        PhysicsProvider.OnObjectStateChange += DetectGrabOrThrow;
    }

    private void OnDestroy() => PhysicsProvider.OnObjectStateChange -= DetectGrabOrThrow;

    private void Update()
    {
        if (handModelBase == null || isLocatingHand || !isGrasped || handModelBase.IsTracked)
            return;
        isLocatingHand = true;
        StartCoroutine(LocatingHand());
    }
    private IEnumerator LocatingHand()
    {
        Vector3 throwDirection = palmTransform.transform.forward;

        while (!handModelBase.IsTracked)
            yield return null;

        Vector3 newForwardDirection = palmTransform.transform.forward;
        yield return null;
        yield return null;

        if (Vector3.Angle(throwDirection, newForwardDirection) > 50.0 && !PhysicsProvider.IsGraspingObject(rb))
        {
            Throw(newForwardDirection * 2f);
        }

        isLocatingHand = false;
    }
    private IEnumerator ThrowAveragedVelocity()
    {
        List<Vector3> velocitySamples = new List<Vector3>();
        Vector3 averageVelocity = Vector3.zero;
        for (int i = 0; i < 8; ++i)
        {
            velocitySamples.Add(rb.velocity);
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(0.04f);
        foreach (Vector3 vector3 in velocitySamples)
            averageVelocity += vector3;
        averageVelocity /= velocitySamples.Count;
        if (averageVelocity.magnitude > minVelocityForThrow)
            Throw(averageVelocity);
    }

    private void Throw(Vector3 averageVelocity) => rb.AddForce(averageVelocity * extraThrowForce, ForceMode.Impulse);

    private void DetectGrabOrThrow(Rigidbody arg1, PhysicsGraspHelper arg2)
    {
        if (isGrasped && !PhysicsProvider.IsGraspingObject(rb))
        {
            isGrasped = false;
            StartCoroutine(ThrowAveragedVelocity());
            graspedHand = null;
        }
        if (arg1 != rb || arg2.GraspState != PhysicsGraspHelper.State.Grasp)
            return;
        isGrasped = true;
        graspedHand = PhysicsProvider.LeftHand.IsGrasping ? PhysicsProvider.LeftHand : PhysicsProvider.RightHand;
        handModelBase = FindObjectsOfType<HandBinder>().First(o => o.Handedness == graspedHand.Handedness);
        palmTransform = graspedHand.GetComponentsInChildren<Transform>(true)[1];
    }
}