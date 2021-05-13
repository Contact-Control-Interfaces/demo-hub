using Maestro;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

    public struct HapticEffect
    {
        public byte Amplitude { get; set; }
        public byte Vibration { get; set; }

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

        public void SetAmplitudeFromIndex(HAND_POSITION index, byte? amplitude)
        {
            switch (index) {
                default: Debug.LogWarning(string.Format("Unimplemented index {0}!", index)); break; /* TODO add other hand positions */

                //case HAND_POSITION.ThumbMiddle:
                case HAND_POSITION.ThumbTip: ThumbAmplitude = amplitude; break;

                //case HAND_POSITION.IndexMiddle:
                case HAND_POSITION.IndexTip: IndexAmplitude = amplitude; break;

                //case HAND_POSITION.MiddleMiddle:
                case HAND_POSITION.MiddleTip: MiddleAmplitude = amplitude; break;

                //case HAND_POSITION.RingMiddle:
                case HAND_POSITION.RingTip: RingAmplitude = amplitude; break;

                //case HAND_POSITION.LittleMiddle:
                case HAND_POSITION.LittleTip: LittleAmplitude = amplitude; break;
            }
        }

        public void SetVibrationEffectFromIndex(HAND_POSITION index, byte? vibrationEffect)
        {
            switch (index) {
                default: Debug.LogWarning(string.Format("Unimplemented index {0}!", index)); break; /* TODO add other hand positions */

                //case HAND_POSITION.ThumbMiddle:
                case HAND_POSITION.ThumbTip: ThumbVibrationEffect = vibrationEffect; break;

                //case HAND_POSITION.IndexMiddle:
                case HAND_POSITION.IndexTip: IndexVibrationEffect = vibrationEffect; break;

                //case HAND_POSITION.MiddleMiddle:
                case HAND_POSITION.MiddleTip: MiddleVibrationEffect = vibrationEffect; break;

                //case HAND_POSITION.RingMiddle:
                case HAND_POSITION.RingTip: RingVibrationEffect = vibrationEffect; break;

                //case HAND_POSITION.LittleMiddle:
                case HAND_POSITION.LittleTip: LittleVibrationEffect = vibrationEffect; break;
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