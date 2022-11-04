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
    public GameObject dropObject;
    public Transform dropOrigin;

    [Header("Drop Object Material")]
    public Material glowOn;

    //HandBool
    private bool hasHand = false;

    //Counter
    [Header("Countdown Components")]
    public TextMeshProUGUI countText;
    public TextMeshProUGUI remainingText;
    public TextMeshProUGUI timeOnText;
    public float baseTime; //test
    public float remainingTime;
    public bool timeOn; //test
    public Image fillImage;

    
    protected int CurrentlyColliding = 0;
    protected Coroutine CurrentCoroutine = null;

    private void Update()
    {

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


