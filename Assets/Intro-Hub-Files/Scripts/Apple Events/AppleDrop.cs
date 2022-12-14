using Leap;
using Maestro;
using Maestro.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class AppleDrop : MonoBehaviour
{
    [Header("Drop Components")]
    bool missed;
    public Apple dropObject;
    public Transform dropOrigin;

    [Header("Drop Object Material")]
    public Material promptFlickerOn;
    public Material promptFlickerOff;

    //HandBool
    //private bool hasHand = false;

    //Counter
    [Header("Countdown Components")]
    public TextType timeOnText;
    public float baseTime; //test
    public float remainingTime;
    public bool timeOn; //test


    
    protected int CurrentlyColliding = 0;
    protected Coroutine CurrentCoroutine = null;

    private IMaestroHand lastHand;
    
    private void Start()
    {
        timeOnText = FindObjectOfType<TextType>();
        //timeOnText.TextGen("Welcome");
        timeOnText.TextGen(" Put your hand under the apple.");
    }

    public void Register(FingerCollider fc)
    {
        CurrentlyColliding++;

        lastHand = fc.hpi;

        if (CurrentCoroutine == null) {
            StartTime();
        }
    }

    public void Deregister(FingerCollider fc)
    {
        CurrentlyColliding--;

        if (CurrentlyColliding <= 0) {
            StopTime();
        }
    }


    #region Material Logic


    public void StartTime()
    {
        if (!dropObject.StillAttachedToTree)
            return; // Only change material, start timer when apple is on the tree

        dropObject.GetComponent<MeshRenderer>().material = promptFlickerOn;
        
        timeOn = true;
        //timeOnText.text = "Hold your hand still.";
        remainingTime = baseTime;
        timeOnText.TextGen("Please hold still...");
        if (CurrentCoroutine != null) {
            StopCoroutine(CurrentCoroutine);
            CurrentCoroutine = null;
        }
        //timeOnText.UpText();
        
        CurrentCoroutine = StartCoroutine(UpdateTimer());
    }

    public void StopTime()
    {
        if (!dropObject.StillAttachedToTree)
            return; // Only change material, start timer when apple is on the tree

        dropObject.GetComponent<MeshRenderer>().material = promptFlickerOff;
        timeOn = false;
        timeOnText.TextGen(" Put your hand under the apple.");
        if (CurrentCoroutine != null) {
            StopCoroutine(CurrentCoroutine);
            CurrentCoroutine = null;
        }

        //timeOnText.BackText();
        ResetDisplay();
    }

    #endregion

    #region Timer

    private IEnumerator UpdateTimer()
    {
        while(remainingTime > 0)
        {
            if (!timeOn)
                break;

            remainingTime--;

            yield return new WaitForSeconds(1f);
        }

        OnEnd();
    }

    private void OnEnd()
    {
        dropObject.Pluck();

        Vector3 offsetFromPalm = lastHand.transforms.MiddleKnuckle.position - dropObject.transform.position;

        float fallDistance = Mathf.Abs(offsetFromPalm.y);
        float a = Mathf.Abs(Physics.gravity.y);
        float timeToFall = Mathf.Sqrt(2 * a * fallDistance) / a;

        Vector3 lateralOffset = Vector3.Scale(offsetFromPalm, new Vector3(1, 0, 1));

        var rb = dropObject.GetComponent<Rigidbody>();
        rb.velocity += lateralOffset / timeToFall;

        timeOnText.TextGen("Apple Dropped");
    }

    private void ResetDisplay()
    {
        remainingTime = baseTime;
    }

    #endregion
}
