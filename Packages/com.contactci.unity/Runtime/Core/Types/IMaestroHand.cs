using Maestro.Vibration;
using System;
using System.Collections;
using System.Collections.Generic;
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
    public struct HandSize
    {
        public float TipSize;
        public float MiddleSize;
        public float KnuckleSize;
    }

    [Serializable]
    public struct HandTransforms : IEnumerable, IEnumerable<Transform>
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

        public Transform PalmBaseThumb;
        public Transform PalmBaseLittle;
        public Transform BetweenIndexMiddle;
        public Transform BetweenRingLittle;
        public Transform PalmCreaseThumb;
        public Transform PalmCreaseMiddle;
        public Transform PalmCreaseLittle;

        public bool FullyDefined {
            get {
                foreach (Transform t in this) {
                    if (t == null) return false;
                }
                return true;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable<Transform>)this).GetEnumerator();
        }

        IEnumerator<Transform> IEnumerable<Transform>.GetEnumerator()
        {
            yield return ThumbTip;
            yield return IndexTip;
            yield return MiddleTip;
            yield return RingTip;
            yield return LittleTip;
            yield return ThumbMiddle;
            yield return IndexMiddle;
            yield return MiddleMiddle;
            yield return RingMiddle;
            yield return LittleMiddle;
            yield return ThumbKnuckle;
            yield return IndexKnuckle;
            yield return MiddleKnuckle;
            yield return RingKnuckle;
            yield return LittleKnuckle;
            yield return PalmBaseThumb;
            yield return PalmBaseLittle;

            yield return BetweenIndexMiddle;
            yield return BetweenRingLittle;
            yield return PalmCreaseThumb;
            yield return PalmCreaseMiddle;
            yield return PalmCreaseLittle;
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
        public IGrabManager grabManager;

        public bool ShowOnlyWhileTouching = true;

        public abstract Transform Palm { get; }

        public IntPtr GlovePointer { get; protected set; }

        public bool Connected { get; protected set; }
        public static bool StartSucceeded { get; protected set; }
        public static bool StartAttempted { get; protected set; }

        protected abstract MaestroHapticContext ProcessHaptics();

        public HandTransforms transforms;

        public virtual void Start()
        {
            if (GlovePointer == IntPtr.Zero) {
                if (whichHand == WhichHand.LeftHand)
                    GlovePointer = MaestroGloveConnector.Instance.GetLeftGlovePointer();
                else
                    GlovePointer = MaestroGloveConnector.Instance.GetRightGlovePointer();
            }

            Connected = MaestroGloveConnector.Instance.isGloveConnected(GlovePointer);

            if (!Connected && !StartAttempted) {
                StartDetection();
            }

            StartCoroutine("LateFixedUpdate");
        }

        public virtual void Update()
        {
            bool stillConnected = MaestroGloveConnector.Instance.isGloveConnected(GlovePointer);

            Connected = stillConnected;
        }

        protected virtual void StartDetection()
        {
            MaestroGloveConnector.Instance.OnDetectionStarted += OnDetectionStarted;

            MaestroGloveConnector.Instance.StartScanningForGloves();

            StartAttempted = true;
        }

        protected virtual void OnDetectionStarted(object source, bool detectionStarted)
        {
            MaestroGloveConnector.Instance.OnDetectionStarted -= OnDetectionStarted;

            StartSucceeded = detectionStarted;

            if (StartSucceeded)
                Debug.Log("Maestro detection service is running.");
            else
                Debug.LogError("Maestro detection service is not running!");
        }

        public virtual IEnumerator LateFixedUpdate()
        {
            for (; ; ) {
                MaestroHapticContext next = ProcessHaptics();
                HapticsApplicator.ApplyHaptics(GlovePointer, next, lastHaptics);
                lastHaptics = next;

                yield return new WaitForFixedUpdate();
            }
        }
    }
}
