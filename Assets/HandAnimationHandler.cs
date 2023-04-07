using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandAnimationHandler : MonoBehaviour
{
    [SerializeField] private int iterationAmount;
    [SerializeField] private int timesPlayed;

    public void CountIteration()
    {
        timesPlayed++;
        if(timesPlayed >= iterationAmount)
        {
            this.gameObject.SetActive(false);
        }
    }
}
