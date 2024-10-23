using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class Football : MonoBehaviour
{

    private float throwTime = 3.5f;

    public float elapsedTime;

    private Coroutine currentRoutine;

    public CustomHapticSender customHaptic;

    public void ThrowTimer()
    {
        elapsedTime = 0;
        currentRoutine = StartCoroutine(ThrowTick());
    }

    public void StopTimer()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }
    }

    private IEnumerator ThrowTick()
    {
       

        do
        {
            elapsedTime += Time.deltaTime;
            yield return new WaitForSeconds(.01f);
        }
        while (elapsedTime < throwTime);
        Debug.Log("Buzz");
        customHaptic.StartHaptics();
        StopTimer();
    }
}
