using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Leap.Unity;
using Maestro;
using UnityEngine;

public class Football : MonoBehaviour
{

    private float throwTime = 3.5f;

    private float elapsedTime;

    private Coroutine currentRoutine;

    public CustomHapticSender sender;

    private MaestroGrabbable grabbable;

    private void Awake()
    {
        grabbable = GetComponent<MaestroGrabbable>();
    }

    public void ThrowTimer()
    {
        int whichHand =  grabbable.graspedHand.Handedness == Chirality.Right ? 0 : 1;
        sender.wristHand = FindObjectsOfType<MaestroHand>().Where(hand => hand.whichHand == (WhichHand)whichHand)
            .FirstOrDefault();
        if (currentRoutine == null)
        {
            currentRoutine = StartCoroutine(ThrowTick());
        }
    }

    public void StopTimer()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }
        sender.StopHaptics();
        elapsedTime = 0;
        Debug.Log("Stopping timer");
    }

    private IEnumerator ThrowTick()
    {
        while (elapsedTime < throwTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Debug.Log("Buzz");
        sender.StartHaptics();
    }
}
