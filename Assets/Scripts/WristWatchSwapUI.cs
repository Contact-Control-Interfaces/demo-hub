using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WristWatchSwapUI : MonoBehaviour
{
   [SerializeField]
   private Image leftHand;
   [SerializeField]
   private Image rightHand;


    public void LightUpGraphic(bool leftHandOn)
    {
        if (leftHandOn)
        {
            Debug.Log("Turn on left hand. Left Hand On: " + leftHand);
            leftHand.color = Color.green;
            rightHand.color = Color.white;
        }
        else
        {
            Debug.Log("Turn on right hand. Left Hand On: " + rightHand);
            rightHand.color = Color.green;
            leftHand.color = Color.white;
        }
    }


}
