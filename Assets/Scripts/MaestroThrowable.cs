using Leap.Unity;
using Leap.Unity.HandsModule;
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
    private Transform palmTransform;
    private HandModelBase handModelBase;
    private float extraThrowForce = 2f;
    private float minVelocityForThrow = 1.4f;
    private bool isLocatingHand;

    private void Start()
    {
        this.PhysicsProvider = UnityEngine.Object.FindObjectOfType<PhysicsProvider>();
        this.rb = this.GetComponent<Rigidbody>();
        this.PhysicsProvider.OnObjectStateChange += new Action<Rigidbody, PhysicsGraspHelper>(this.DetectGrabOrThrow);
    }

    private void OnDestroy() => this.PhysicsProvider.OnObjectStateChange -= new Action<Rigidbody, PhysicsGraspHelper>(this.DetectGrabOrThrow);

    private IEnumerator ThrowAveragedVelocity()
    {
        List<Vector3> velocitySamples = new List<Vector3>();
        Vector3 averageVelocity = Vector3.zero;
        for (int i = 0; i < 8; ++i)
        {
            velocitySamples.Add(this.rb.velocity);
            yield return (object)new WaitForFixedUpdate();
        }
        yield return (object)new WaitForSeconds(0.04f);
        foreach (Vector3 vector3 in velocitySamples)
            averageVelocity += vector3;
        averageVelocity /= (float)velocitySamples.Count;
        if ((double)averageVelocity.magnitude > (double)this.minVelocityForThrow)
            this.Throw(averageVelocity);
    }

    private void Throw(Vector3 averageVelocity) => this.rb.AddForce(averageVelocity * this.extraThrowForce, ForceMode.Impulse);

    private void DetectGrabOrThrow(Rigidbody arg1, PhysicsGraspHelper arg2)
    {
        if (this.isGrasped && !this.PhysicsProvider.IsGraspingObject(this.rb))
        {
            this.isGrasped = false;
            this.StartCoroutine(this.ThrowAveragedVelocity());
            this.graspedHand = (PhysicsHand)null;
        }
        if (!((UnityEngine.Object)arg1 == (UnityEngine.Object)this.rb) || arg2.GraspState != PhysicsGraspHelper.State.Grasp)
            return;
        this.isGrasped = true;
        this.graspedHand = this.PhysicsProvider.LeftHand.IsGrasping ? this.PhysicsProvider.LeftHand : this.PhysicsProvider.RightHand;
        this.handModelBase = (HandModelBase)((IEnumerable<HandBinder>)UnityEngine.Object.FindObjectsOfType<HandBinder>()).First<HandBinder>((Func<HandBinder, bool>)(o => o.Handedness == this.graspedHand.Handedness));
        this.palmTransform = this.graspedHand.GetComponentsInChildren<Transform>(true)[1];
    }

    private void FixedUpdate()
    {
    }

    private void Update()
    {
        if (!((UnityEngine.Object)this.handModelBase != (UnityEngine.Object)null) || this.isLocatingHand || !this.isGrasped || this.handModelBase.IsTracked)
            return;
        this.isLocatingHand = true;
        this.StartCoroutine(this.LocatingHand());
    }

    private IEnumerator LocatingHand()
    {
        MaestroThrowable maestroThrowable = this;
        Vector3 position = maestroThrowable.handModelBase.transform.position;
        Vector3 vector3 = maestroThrowable.handModelBase.transform.position - maestroThrowable.transform.position;
        Vector3 throwDirection = maestroThrowable.palmTransform.transform.forward;
        while (!maestroThrowable.handModelBase.IsTracked)
            yield return (object)null;
        Vector3 newForwardDirection = maestroThrowable.palmTransform.transform.forward;
        yield return (object)null;
        yield return (object)null;
        if ((double)Vector3.Angle(throwDirection, newForwardDirection) > 50.0 && !maestroThrowable.PhysicsProvider.IsGraspingObject(maestroThrowable.rb))
            maestroThrowable.Throw(newForwardDirection * 2f);
        maestroThrowable.isLocatingHand = false;
    }
}