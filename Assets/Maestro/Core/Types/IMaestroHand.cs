using Maestro.Vibration;
using System;
using System.Collections;
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

    [Serializable]
    public struct HapticEffect
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
    }

    [Serializable]
    public struct HandSize
    {
        public float TipSize;
        public float MiddleSize;
        public float KnuckleSize;
    }

    [Serializable]
    public struct HandTransforms
    {
        public Transform ThumbTip;
        public Transform IndexTip;
        public Transform MiddleTip;
        public Transform RingTip;
        public Transform LittleTip;
        public Transform ThumbMiddle;
        public Transform IndexMiddle;
        public Transform MiddleMiddle;
        public Transform RingMiddle;
        public Transform LittleMiddle;
        public Transform ThumbKnuckle;
        public Transform IndexKnuckle;
        public Transform MiddleKnuckle;
        public Transform RingKnuckle;
        public Transform LittleKnuckle;
        public Transform PalmBase;
    }

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
            switch (index.finger) {
                default: Debug.LogWarning(string.Format("Unimplemented index {0}!", index)); break; /* TODO add other hand positions */

                case WhichFinger.Thumb: ThumbAmplitude = amplitude; break;
                case WhichFinger.Index: IndexAmplitude = amplitude; break;
                case WhichFinger.Middle: MiddleAmplitude = amplitude; break;
                case WhichFinger.Ring: RingAmplitude = amplitude; break;
                case WhichFinger.Little: LittleAmplitude = amplitude; break;
            }
        }

        public void SetVibrationEffectFromIndex(MaestroIndex index, VibrationEffect vibrationEffect)
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
                HapticsApplicator.ApplyHaptics(parentGloveBehavior.GetPointer(), next, lastHaptics);
                lastHaptics = next;

                yield return new WaitForFixedUpdate();
            }
        }
    }
}
