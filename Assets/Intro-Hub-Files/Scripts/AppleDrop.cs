using Leap;
using Maestro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Maestro.Poser;
using Image = UnityEngine.UI.Image;

public class AppleDrop : MonoBehaviour
{

    //Materials
    public Material enteredMaterial;
    public Material inMaterial;
    public Material exitMaterial;
    public Material emptyMaterial;

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

    //UIElements
    public Image fillImage;

    public HandBones handBones;

    private void Update()
    {
        if(!hasHand)
        {
            this.GetComponent<Renderer>().material = emptyMaterial;
        }

        if (timeOn)
        {
            remainingTime = baseTime;
            StartCoroutine(UpdateTimer());
        }
        
    }

    public void Register(FingerCollider fc)
    {
       // if (fc.hpi.whichHand == whichHand)
        //{
        //    SetState(fc, true);
        //}
    }

    public void Deregister(FingerCollider fc)
    {
        // Instead we wait until you uncurl
        //SetState(fc, false);
    }

    public void SetState(FingerCollider fc, bool state)
    {
        switch (fc.index.finger)
        {
            //default: return;
            //case WhichFinger.Thumb: ClampThumb = state; break;
            //case WhichFinger.Index: ClampIndex = state; break;
            //case WhichFinger.Middle: ClampMiddle = state; break;
            //case WhichFinger.Ring: ClampRing = state; break;
            //case WhichFinger.Little: ClampLittle = state; break;
        }
    }


    #region Material Logic

    //Functions below check if an object with a finger collider are within the trigger's bounds.
    //Material is changed accordingly.
    /*    private void OnTriggerEnter(Collider other)
        {
            if(other.GetComponent<FingerCollider>() && !hasHand)
            {
                hasHand = true;
                this.GetComponent<Renderer>().material = inMaterial;
                timeOn = true;
                timeOnText.text = "Time On";
                //hasHand = true;

            }
        }*/

    public void StartTime()
    {
        if (hasHand == false)
        {
            hasHand = true;
            this.GetComponent<Renderer>().material = inMaterial;
            timeOn = true;
            timeOnText.text = "Time On";
            //hasHand = true;
        }
    }



/*    private void OnTriggerExit(Collider other)
    {
        this.GetComponent<Renderer>().material = exitMaterial;
        timeOn = false;
        timeOnText.text = "Time Off";
        hasHand = false;
    }
*/
    public void StopTime()
    {
        this.GetComponent<Renderer>().material = exitMaterial;
        timeOn = false;
        timeOnText.text = "Time Off";
        hasHand = false;
    }

    #endregion

    #region Timer

    private IEnumerator UpdateTimer()
    {
        while (remainingTime > 0)
        {
            if (timeOn)
            {
                fillImage.fillAmount = Mathf.InverseLerp(0, baseTime, remainingTime);
                remainingTime--;
                remainingText.text = remainingTime.ToString();

                if(remainingTime == 0)
                {
                    dropObject.GetComponent<Rigidbody>().useGravity = true;
                }
            }
            else
            {
                remainingTime = baseTime;
                fillImage.fillAmount = Mathf.InverseLerp(0, baseTime, remainingTime);
                yield break;
            }
           
            yield return new WaitForSeconds(1f);
        }
        OnEnd();

    }

    private void OnEnd()
    {
        
    }


    #endregion

/*           if (remainingTime <= 0)
        {
            dropObject.GetComponent<Rigidbody>().useGravity = true;
        }
        else
{
    remainingTime = baseTime;*/
}


