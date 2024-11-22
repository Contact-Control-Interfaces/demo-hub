using Leap.Unity;
using Leap.Unity.HandsModule;
using Leap.Unity.PhysicalHands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class MaestroGrabbable : MonoBehaviour
{
    [SerializeField]
    private bool throwHelpersEnabled = true;
    private Rigidbody rb;
    private PhysicalHandsManager physicalHandsManager;
    private ContactHand graspedHand;
    private Transform palmTransform;
    private HandModelBase handModelBase;
    private bool isGrasped;
    private bool isLocatingHand;
    private float extraThrowForce = 2f;
    private float minVelocityForThrow = 1.4f;
    [SerializeField]
    public UnityEvent OnRelease;
    [SerializeField]
    public UnityEvent OnGrab;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        physicalHandsManager = FindObjectOfType<PhysicalHandsManager>();
        physicalHandsManager.onGrab.AddListener(OnObjectGrabbed);
        physicalHandsManager.onGrabExit.AddListener(OnObjectReleased);
    }

    private void OnObjectReleased(ContactHand hand, Rigidbody rb)
    {
        if (rb == this.rb)
        {
            rb.isKinematic = false;
            StartCoroutine(ThrowAveragedVelocity());
            graspedHand = null;
        }
    }

    private void OnObjectGrabbed(ContactHand hand, Rigidbody rb)
    {
        if (rb == this.rb)
        {
            graspedHand = hand;
            GrabbedHand();
            palmTransform = graspedHand.GetComponentsInChildren<Transform>(true)[1];
            OnGrab?.Invoke();
        }
    }

    private void Update()
    {
        if (!throwHelpersEnabled)
            return;
        if (graspedHand == null || isLocatingHand || !isGrasped || handModelBase.IsTracked)
            return;
        isLocatingHand = true;
        StartCoroutine(LocatingHand());
    }
    private IEnumerator LocatingHand()
    {
        Vector3 throwDirection = palmTransform.transform.forward;

        while (graspedHand != null && !graspedHand.Tracked)
            yield return null;

        Vector3 newForwardDirection = palmTransform.transform.forward;
        yield return null;
        yield return null;

        if (Vector3.Angle(throwDirection, newForwardDirection) > 50.0 && graspedHand != null)
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

    public ContactHand GrabbedHand()
    {
        return graspedHand;
    }
}