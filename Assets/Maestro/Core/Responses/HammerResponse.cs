using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerResponse : HapticResponse {

    public int AmpScale = 5000;
    public float DecayRate = 0.5f;
    public float HitDelay = 0.5f;

    public float StrengthScale = 0.0f;

    public float CurrentDelay = 0.0f;
    public byte CurrentAmp = 0;

    private Vector3 impulseSum = Vector3.zero;

    public MaestroHand right;

    public float minImpulse = 1.0f;

    new void Start()
    {
        base.Start();
        CurrentAmp = 40;
    }

    protected override void GetResponse(MaestroInteractable parent)
    {

        if (impulseSum.magnitude > minImpulse)
        {
            CurrentAmp = (byte)Mathf.Min(255, parent.Amplitude + ((impulseSum.magnitude / (minImpulse*100)) - 1));
            CurrentDelay = 0f;
            Debug.Log("BONK: " + CurrentAmp);
        }

        if (CurrentAmp > parent.Amplitude)
        {
            parent.ResponseMotorAmplitude = CurrentAmp;
            parent.ResponseVibrationEffect = (byte)Mathf.Min(128, (CurrentAmp*2));

            
        } else
        {
            parent.ResponseMotorAmplitude = parent.ResponseVibrationEffect = null;
        }

        if (CurrentAmp > parent.Amplitude && CurrentDelay > (HitDelay * (StrengthScale + 1)))
        {
            CurrentAmp = (byte)(CurrentAmp * DecayRate);
            if (CurrentAmp < parent.Amplitude)
                CurrentAmp = parent.Amplitude;
        }

        impulseSum = Vector3.zero;
        CurrentDelay += Time.deltaTime;
    }

    public void OnCollisionEnter(Collision collision)
    {
		impulseSum += collision.relativeVelocity;
    }
}
