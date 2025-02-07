using Leap.Unity;
using Maestro;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WristGaze : MonoBehaviour
{
    [Header("Gaze Objects")]
    public Transform playerHead;
    public GameObject wristToggle;
    public WristMenu wristMenu;
    public ObjectFill fillHandler;

    [Header("Gaze Distance")]
    private float curDistance;
    private float wristThreshold = 0.80f;
    private float triggerDistance = .4f;

    [Header("Wrist Band Objects")]
    public GameObject wristBandObject;
    public Material wristBandOn;
    public Material wristBandOff;


    [Header("Gaze Buffer")]
    public int interactableCount;
    public int bufferLength = 2;
    public bool wristReady;
    private Coroutine bufferCoroutine;

    void Start()
    {
        wristReady = true;
        if (wristMenu == null)
        {
            wristMenu = FindAnyObjectByType<WristMenu>();
        }
    }

    public bool CheckWrist(Vector3 A, Vector3 B)
    {
        curDistance = Vector3.Distance(wristToggle.transform.position, playerHead.transform.position);
        var lookPercentage = Vector3.Dot(A.normalized, B.normalized);
        ColorAdjust(lookPercentage);

        if (lookPercentage > wristThreshold)
        {
            return true;
        }

        return false;
    }

    public void ColorAdjust(float colorValue)
    {
        float valueClamp = Mathf.Clamp01(colorValue);
        wristBandOn.SetFloat("_DotProduct", valueClamp);
    }

    private void Update()
    {
        if (!wristMenu.menu.Active && wristToggle.activeSelf && wristReady)
        {
            if (CheckWrist(this.transform.forward, wristToggle.transform.up) && curDistance <= triggerDistance)
            {
                fillHandler.Fill(false);
            }

            else
            {
                fillHandler.StopFill();
            }
        }
    }


    public void startBuffer()
    {
        if (bufferCoroutine == null)
        {
            bufferCoroutine = StartCoroutine(wristBufferTimer());
        }
    }

    public void wristNotReady()
    {
        if (wristReady)
        {
            Debug.Log("Stop the wrist");
            wristBandObject.GetComponent<Renderer>().material = wristBandOff;
            wristReady = false;
        }
    }

    private IEnumerator wristBufferTimer()
    {
        //Print the time of when the function is first called.
        Debug.Log("Started Coroutine at timestamp : " + Time.time);

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(bufferLength);

        //After we have waited 5 seconds print the time again.
        Debug.Log("Finished Coroutine at timestamp : " + Time.time);
        bufferCoroutine = null;
        wristReady = true;
        wristBandObject.GetComponent<Renderer>().material = wristBandOn;
    }
}
