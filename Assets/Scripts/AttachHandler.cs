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

    public bool leftWatchOn;
    public WristWatchSwapUI swapPanel;

    public void Start()
    {
        leftAttach.SetActive(false);
        rightAttach.SetActive(false);
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
            rightAttach.SetActive(true);
        }
        else
        {
            leftAttach.SetActive(true);
        }
    }

    private void NotGrabbed()
    {
        Debug.Log("Let go");

        if (grabbingHand.Handedness == Leap.Unity.Chirality.Left)
        {
            rightAttach.SetActive(false);
        }
        else
        {
            leftAttach.SetActive(false);
        }
        grabbingHand = null;
    }

    public void SwapPressed()
    {
        leftWatchOn = !leftWatchOn;
        if(leftWatchOn)
        {
            rightAttach.GetComponent<AttachPoint>().EnableWrist(leftWatchOn);
            leftAttach.GetComponent<AttachPoint>().DisableWrist();
        }                        
        else
        {
            leftAttach.GetComponent<AttachPoint>().EnableWrist(leftWatchOn);
            rightAttach.GetComponent<AttachPoint>().DisableWrist();
        }

        swapPanel.LightUpGraphic(leftWatchOn);

    }




}
