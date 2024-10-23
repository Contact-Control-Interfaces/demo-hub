using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Maestro;
using UnityEngine;

public class Football : MonoBehaviour
{

    private float throwTime = 3.5f;

    private float elapsedTime;

    private Coroutine currentRoutine;

    public CustomHapticSender sender;

    public void ThrowTimer()
    {
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
