using Leap.Unity.PhysicalHands;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachHandler : MonoBehaviour
{
    [SerializeField] GameObject leftAttach;
    [SerializeField] GameObject rightAttach;
    [SerializeField] MaestroGrabbable spawnedWatch;
    [SerializeField] ContactHand grabbingHand;

    public void Update()
    {
        if(spawnedWatch != null)
        Debug.Log(spawnedWatch.name);
    }

    public void ListenToSpawner(MaestroGrabbable grabbable)
    {
        spawnedWatch = grabbable;
        spawnedWatch.OnGrab.AddListener(HandleGrab);
        spawnedWatch.OnRelease.AddListener(NotGrabbed);
    }

    private void HandleGrab()
    {
        if (spawnedWatch != null)
            grabbingHand = spawnedWatch.GrabbedHand();

        if (grabbingHand != null && grabbingHand.Handedness == Leap.Unity.Chirality.Left)
        {
            leftAttach.SetActive(false);
        }
        else
        {
            rightAttach.SetActive(false);
        }
    }

    private void NotGrabbed()
    {
        if (grabbingHand.Handedness == Leap.Unity.Chirality.Left)
        {
            leftAttach.SetActive(true);
        }
        else
        {
            rightAttach.SetActive(true);
        }

        grabbingHand = null;
    }



}
