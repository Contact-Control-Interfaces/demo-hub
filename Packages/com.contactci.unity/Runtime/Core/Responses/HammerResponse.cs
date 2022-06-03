using Maestro.Vibration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public class HammerResponse : HapticResponse
    {

        public int AmpScale = 5000;
        public float DecayRate = 0.5f;
        public float HitDelay = 0.5f;

        public float StrengthScale = 0.0f;

        public float CurrentDelay = 0.0f;
        public byte CurrentAmp = 0;

        private Vector3 impulseSum = Vector3.zero;

        public IMaestroHand right;

        public float minImpulse = 1.0f;

        new void Start()
        {
            base.Start();
            CurrentAmp = 40;
        }

        protected override void GetResponse(MaestroInteractable parent)
        {
            if (impulseSum.magnitude > minImpulse) {
                CurrentAmp = (byte)Mathf.Min(255, parent.haptics.Amplitude + ((impulseSum.magnitude / (minImpulse * 100)) - 1));
                CurrentDelay = 0f;
                Debug.Log("BONK: " + CurrentAmp);
            }

            if (CurrentAmp > parent.haptics.Amplitude) {
                parent.ResponseMotorAmplitude = CurrentAmp;
                parent.ResponseVibrationEffect = new StrongBuzz();
            } else {
                parent.ResponseMotorAmplitude = null;
                parent.ResponseVibrationEffect = null;
            }

            if (CurrentAmp > parent.haptics.Amplitude && CurrentDelay > (HitDelay * (StrengthScale + 1))) {
                CurrentAmp = (byte)(CurrentAmp * DecayRate);
                if (CurrentAmp < parent.haptics.Amplitude)
                    CurrentAmp = parent.haptics.Amplitude;
            }

            impulseSum = Vector3.zero;
            CurrentDelay += Time.deltaTime;
        }

        public void OnCollisionEnter(Collision collision)
        {
            impulseSum += collision.relativeVelocity;
        }
    }
}
