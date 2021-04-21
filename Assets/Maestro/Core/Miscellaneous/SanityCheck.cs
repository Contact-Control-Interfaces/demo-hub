using Maestro.Haptics.Vibration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SanityCheck : MonoBehaviour
{
    private MaestroInteractable knob;
    public MaestroHand hand;
    private VibrateOnCollideBehaviour vocb;
    public FingerTipCollider indexFTC;

    private string lastDisplayed;

    // Start is called before the first frame update
    void Start()
    {
        knob = this.GetComponent<MaestroInteractable>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (vocb == null)
            vocb = hand.IndexTip.GetComponent<VibrateOnCollideBehaviour>();

        if (vocb != null && indexFTC != null && (indexFTC.touching != null && indexFTC.touching.Equals(knob)))
        {
            string toDisplay = vocb.VibrationEffect + " " + knob.getVibrationEffect() + " " + knob.VibrationEffect;
            if (vocb.VibrationEffect != null && vocb.VibrationEffect.Value != knob.getVibrationEffect())
            {
                toDisplay += "\r\nERROR";
            }
            MaestroBLEUI.SetRightText(toDisplay);

            /*if (!toDisplay.Equals(lastDisplayed))
            {
                Debug.Log(toDisplay);
                lastDisplayed = toDisplay;

                if (vocb.VibrationEffect != null && vocb.VibrationEffect.Value != knob.getVibrationEffect())
                {
                    Debug.Log("WHOA THERE");
                }
            }*/
        }
    }
}
