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
    bool missed;

    //Materials
    public Material enteredMaterial;
    public Material inMaterial;
    public Material exitMaterial;
    public Material emptyMaterial;

    public Material glowOn;

    //HandBool
    private bool hasHand = false;

    //Counter
    public TextMeshProUGUI countText;
    public TextMeshProUGUI remainingText;
    public TextMeshProUGUI timeOnText;
    public float baseTime;
    public float remainingTime;
    public bool timeOn;

    //ObjecttoDrop

    public GameObject dropObject;
    public Transform dropOrigin;

    //UIElements
    public Image fillImage;

    protected int CurrentlyColliding = 0;
    protected Coroutine CurrentCoroutine = null;

    private void Update()
    {
        if(!hasHand)
        {
            this.GetComponent<Renderer>().material = emptyMaterial;
        }
    }

    public void Register(FingerCollider fc)
    {
        CurrentlyColliding++;

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
        this.GetComponent<Renderer>().material = inMaterial;
        timeOn = true;
        timeOnText.text = "Time On";
        remainingTime = baseTime;

        if (CurrentCoroutine != null) {
            StopCoroutine(CurrentCoroutine);
            CurrentCoroutine = null;
        }
        CurrentCoroutine = StartCoroutine(UpdateTimer());
    }

    public void StopTime()
    {
        this.GetComponent<Renderer>().material = exitMaterial;
        timeOn = false;
        timeOnText.text = "Time Off";

        if (CurrentCoroutine != null) {
            StopCoroutine(CurrentCoroutine);
            CurrentCoroutine = null;
        }

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

            fillImage.fillAmount = (float)remainingTime / baseTime;
            remainingText.text = remainingTime.ToString();

            remainingTime--;

            yield return new WaitForSeconds(1f);
        }

        fillImage.fillAmount = 0f;
        remainingText.text = "0";

        OnEnd();
    }

    private void OnEnd()
    {
        dropObject.GetComponent<Rigidbody>().isKinematic = false;
    }

    private void ResetDisplay()
    {
        remainingTime = baseTime;
        fillImage.fillAmount = 0f;
        remainingText.text = "";
    }

    #endregion


    
   
}


