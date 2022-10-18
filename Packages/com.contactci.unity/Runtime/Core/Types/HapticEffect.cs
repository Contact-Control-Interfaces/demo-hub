using System;
using Maestro.Vibration;
using UnityEngine;

namespace Maestro
{
    [Serializable]
    public class HapticEffect
    {
        public byte Amplitude;
        [SerializeReference]
        public VibrationEffect Vibration;

        public const byte FORCE_FEEDBACK_MAX_AMPLITUDE = 255;
        public const byte FORCE_FEEDBACK_MIN_AMPLITUDE = 0;

        public int CompareAmplitudesFirst(HapticEffect other)
        {
            if (this.Amplitude != other.Amplitude) {
                return this.Amplitude.CompareTo(other.Amplitude);
            } else {
                // TODO this comparison makes no sense
                return this.Vibration.CompareTo(other.Vibration);
            }
        }

        public int CompareVibrationEffectsFirst(HapticEffect other)
        {
            if (!this.Vibration.Equals(other.Vibration)) {
                // TODO this comparison makes no sense
                return this.Vibration.CompareTo(other.Vibration);
            } else {
                return this.Amplitude.CompareTo(other.Amplitude);
            }
        }

        public HapticEffect Copy()
        {
            return new HapticEffect() { Amplitude = this.Amplitude, Vibration = this.Vibration };
        }
        
        public override bool Equals(object obj)
        {
            if (!(obj is HapticEffect other))
                return false;
            return this.Amplitude == other.Amplitude && this.Vibration == other.Vibration;
        }

        public override int GetHashCode()
        {
            int code = 71;
            code = code * 17 ^ Amplitude;
            code = code * 17 ^ Vibration.GetHashCode();
            return code;
        }
    }
}