using Maestro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WristTimerTrigger : MonoBehaviour
{
    public WristGaze wristGaze;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.GetComponent<FingerCollider>())
        {
            wristGaze.interactableCount++;
            wristGaze.wristNotReady();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.GetComponent<FingerCollider>())
        {
            Debug.Log(other.tag);
            Debug.Log(other.gameObject.name);
            wristGaze.interactableCount--;
            if (wristGaze.interactableCount <= 0)
            {
                wristGaze.startBuffer();
            }
        }
    }
}
