using Maestro;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace Maestro
{
    public enum WhichHand
    {
        RightHand, LeftHand
    };

    public enum InteractionPriority
    {
        PrioritizeAmplitude, PrioritizeVibrationEffect /*, Cascade, Importance*/
    };

    [Serializable]
    public struct HapticEffect
    {
        public byte Amplitude;
        public byte Vibration;

        public const byte FORCE_FEEDBACK_MAX_AMPLITUDE = 255;
        public const byte FORCE_FEEDBACK_MIN_AMPLITUDE = 0;
        
        public const byte VIBRATION_MAX_ID = 128; //will change when vibration is handled differently
        public const byte VIBRATION_MIN_ID = 0;



    }

    public struct MaestroHapticContext
    {
        public byte? ThumbAmplitude { get; set; }
        public byte? IndexAmplitude { get; set; }
        public byte? MiddleAmplitude { get; set; }
        public byte? RingAmplitude { get; set; }
        public byte? LittleAmplitude { get; set; }

        public byte? ThumbVibrationEffect { get; set; }
        public byte? IndexVibrationEffect { get; set; }
        public byte? MiddleVibrationEffect { get; set; }
        public byte? RingVibrationEffect { get; set; }
        public byte? LittleVibrationEffect { get; set; }

        public void SetAllAmplitudes(byte? amplitude)
        {
            ThumbAmplitude = amplitude;
            IndexAmplitude = amplitude;
            MiddleAmplitude = amplitude;
            RingAmplitude = amplitude;
            LittleAmplitude = amplitude;
        }

        public void SetAllVibrationEffects(byte? vibrationEffect)
        {
            ThumbVibrationEffect = vibrationEffect;
            IndexVibrationEffect = vibrationEffect;
            MiddleVibrationEffect = vibrationEffect;
            RingVibrationEffect = vibrationEffect;
            LittleVibrationEffect = vibrationEffect;
        }

        public void SetAmplitudeFromIndex(MaestroIndex index, byte? amplitude)
        {
            switch (index.finger) {
                default: Debug.LogWarning(string.Format("Unimplemented index {0}!", index)); break; /* TODO add other hand positions */

                case WhichFinger.Thumb: ThumbAmplitude = amplitude; break;
                case WhichFinger.Index: IndexAmplitude = amplitude; break;
                case WhichFinger.Middle: MiddleAmplitude = amplitude; break;
                case WhichFinger.Ring: RingAmplitude = amplitude; break;
                case WhichFinger.Little: LittleAmplitude = amplitude; break;
            }
        }

        public void SetVibrationEffectFromIndex(MaestroIndex index, byte? vibrationEffect)
        {
            switch (index.finger) {
                default: Debug.LogWarning(string.Format("Unimplemented index {0}!", index)); break; /* TODO add other hand positions */

                case WhichFinger.Thumb: ThumbVibrationEffect = vibrationEffect; break;
                case WhichFinger.Index: IndexVibrationEffect = vibrationEffect; break;
                case WhichFinger.Middle: MiddleVibrationEffect = vibrationEffect; break;
                case WhichFinger.Ring: RingVibrationEffect = vibrationEffect; break;
                case WhichFinger.Little: LittleVibrationEffect = vibrationEffect; break;
            }
        }
    }

    public abstract class IMaestroHand : MonoBehaviour
    {
        public InteractionPriority interactionPriority {
            get;
            protected set;
        }

        public MaestroHapticContext lastHaptics {
            get;
            protected set;
        }

        public WhichHand whichHand;

        public abstract Transform Palm { get; }

        protected MaestroGloveBehaviour parentGloveBehavior { get; set; }

        protected abstract MaestroHapticContext ProcessHaptics();

        public virtual void Start()
        {
            // Get parent glove behavior to retrieve pointer
            parentGloveBehavior = GetComponentInParent<MaestroGloveBehaviour>();

            // Log error if no parent exists
            if (parentGloveBehavior == null)
                Debug.LogError("No parent glove behavior found for IMaestroHand!");

            StartCoroutine("LateFixedUpdate");
        }

        public virtual IEnumerator LateFixedUpdate()
        {
            for (; ; ) {
                MaestroHapticContext next = ProcessHaptics();
                MaestroNativeWrapper.SetHapticsFromContexts(parentGloveBehavior.GetPointer(), next, lastHaptics);
                lastHaptics = next;

                yield return new WaitForFixedUpdate();
            }
        }
    }
}