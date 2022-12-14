using Maestro.Vibration;
using UnityEngine;

namespace Maestro
{
    public struct MaestroHapticContext
    {
        public byte? ThumbAmplitude { get; set; }
        public byte? IndexAmplitude { get; set; }
        public byte? MiddleAmplitude { get; set; }
        public byte? RingAmplitude { get; set; }
        public byte? LittleAmplitude { get; set; }

        public VibrationEffect ThumbVibrationEffect { get; set; }
        public VibrationEffect IndexVibrationEffect { get; set; }
        public VibrationEffect MiddleVibrationEffect { get; set; }
        public VibrationEffect RingVibrationEffect { get; set; }
        public VibrationEffect LittleVibrationEffect { get; set; }

        private static MaestroHapticContext? _zero;
        public static MaestroHapticContext zero {
            get {
                if (!_zero.HasValue) {
                    MaestroHapticContext temp = new MaestroHapticContext();
                    temp.SetAllAmplitudes(null);
                    temp.SetAllVibrationEffects(VibrationEffect.None);
                    _zero = temp;
                }
                return _zero.Value;
            }
        }

        public void SetAllAmplitudes(byte? amplitude)
        {
            ThumbAmplitude = amplitude;
            IndexAmplitude = amplitude;
            MiddleAmplitude = amplitude;
            RingAmplitude = amplitude;
            LittleAmplitude = amplitude;
        }

        public void SetAllVibrationEffects(VibrationEffect vibrationEffect)
        {
            ThumbVibrationEffect = vibrationEffect;
            IndexVibrationEffect = vibrationEffect;
            MiddleVibrationEffect = vibrationEffect;
            RingVibrationEffect = vibrationEffect;
            LittleVibrationEffect = vibrationEffect;
        }

        public void SetAmplitudeFromIndex(MaestroIndex index, byte? amplitude)
        {
            switch (index.finger)
            {
                default:
                    Debug.LogWarning(string.Format("Unimplemented index {0}!", index));
                    break; /* TODO add other hand positions */

                case WhichFinger.Thumb:
                    ThumbAmplitude = amplitude;
                    break;
                case WhichFinger.Index:
                    IndexAmplitude = amplitude;
                    break;
                case WhichFinger.Middle:
                    MiddleAmplitude = amplitude;
                    break;
                case WhichFinger.Ring:
                    RingAmplitude = amplitude;
                    break;
                case WhichFinger.Little:
                    LittleAmplitude = amplitude;
                    break;
            }
        }

        public void SetVibrationEffectFromIndex(MaestroIndex index, VibrationEffect vibrationEffect)
        {
            switch (index.finger)
            {
                default:
                    Debug.LogWarning(string.Format("Unimplemented index {0}!", index));
                    break; /* TODO add other hand positions */

                case WhichFinger.Thumb:
                    ThumbVibrationEffect = vibrationEffect;
                    break;
                case WhichFinger.Index:
                    IndexVibrationEffect = vibrationEffect;
                    break;
                case WhichFinger.Middle:
                    MiddleVibrationEffect = vibrationEffect;
                    break;
                case WhichFinger.Ring:
                    RingVibrationEffect = vibrationEffect;
                    break;
                case WhichFinger.Little:
                    LittleVibrationEffect = vibrationEffect;
                    break;
            }
        }
    }
}