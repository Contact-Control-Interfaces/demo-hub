using Leap;
using Maestro;
using Maestro.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using Maestro.Vibration;
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

    [Header("Haptics")]
    [Tooltip("Haptic effect when hand enters the trigger area")]
    public HapticEffect EnterHaptics = new HapticEffect{ Amplitude = 50, Vibration = new SoftBump(WideThreeOptions._30){OneShot = true} };
    
    protected int CurrentlyColliding = 0;
    protected Coroutine CurrentCoroutine = null;

    private IMaestroHand lastHand;
    private MaestroInteractable interactable;
    
    private void Start()
    {
        timeOnText = FindObjectOfType<TextType>();
        //timeOnText.TextGen("Welcome");
        timeOnText.TextGen(" Place your hand under the apple.");
        interactable = GetComponent<MaestroInteractable>();
        interactable.SendHapticsToWholeHand = true;
    }

    public void Register(FingerCollider fc)
    {
        interactable.SetHapticOverride(EnterHaptics);
        CurrentlyColliding++;

        lastHand = fc.hpi;

        TryStartTime();
    }

    public void Deregister(FingerCollider fc)
    {
        interactable.ResetOverride();
        CurrentlyColliding--;

        if (CurrentlyColliding <= 0) {
            StopTime();
        }
    }

    public void TryStartTime()
    {
        if (CurrentCoroutine == null && CurrentlyColliding > 0) {
            StartTime();
        }
    }

    void OnDisable()
    {
        if (CurrentCoroutine != null) {
            StopCoroutine(CurrentCoroutine);
            CurrentCoroutine = null;
        }
    }

    #region Material Logic


    public void StartTime()
    {
        if (!dropObject.StillAttachedToTree)
            return; // Only change material, start timer when apple is on the tree
        
        dropObject.GetComponent<MeshRenderer>().material = promptFlickerOn;
        
        timeOn = true;
        remainingTime = baseTime;
        timeOnText.TextGen("Please hold still...", true);
        if (CurrentCoroutine != null) {
            StopCoroutine(CurrentCoroutine);
            CurrentCoroutine = null;
        }
        //timeOnText.UpText();
        
        if (this.gameObject.activeInHierarchy)
            CurrentCoroutine = StartCoroutine(UpdateTimer());
    }

    public void StopTime()
    {
        interactable.ResetOverride();
        
        if (!dropObject.StillAttachedToTree)
            return; // Only change material, start timer when apple is on the tree

        dropObject.GetComponent<MeshRenderer>().material = promptFlickerOff;
        timeOn = false;
        timeOnText.TextGen("Place your hand under the apple.", true);
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
        if (dropObject == null)
            return;

        dropObject.Pluck();
        interactable.ResetOverride();

        Vector3 offsetFromPalm = lastHand.transforms.MiddleMiddle.position - dropObject.transform.position;

        float fallDistance = Mathf.Abs(offsetFromPalm.y);
        float a = Mathf.Abs(Physics.gravity.y);
        float timeToFall = Mathf.Sqrt(2 * a * fallDistance) / a;

        Vector3 lateralOffset = Vector3.Scale(offsetFromPalm, new Vector3(1, 0, 1));

        var rb = dropObject.GetComponent<Rigidbody>();
        rb.velocity += lateralOffset / timeToFall;

        CurrentCoroutine = null;
    }

    private void ResetDisplay()
    {
        remainingTime = baseTime;
    }

    #endregion
}
