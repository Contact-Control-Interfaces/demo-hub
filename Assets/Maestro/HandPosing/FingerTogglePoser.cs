using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Maestro.Vibration;
using UnityEngine;
using UnityEngine.Events;

namespace Maestro
{
    [Serializable]
    public class TrackEvent : UnityEvent<Vector3> { }

    public class FingerTogglePoser : Poser
    {
        public WhichHand whichHand;

        public bool ClampThumb;
        public bool ClampIndex;
        public bool ClampMiddle;
        public bool ClampRing;
        public bool ClampLittle;

        private bool isAnythingClamped;
        private bool lastIsAnythingClamped;

        [Header("Snapping")]
        public bool locked;
        public Transform SnapTransform;
        public UnityEvent onLock;
        public UnityEvent onFree;
        public TrackEvent trackRealHand;

        [Header("Debug")]
        public bool autoUnlock = true;

        public void Register(FingerCollider fc)
        {
            if (fc.hpi.whichHand == whichHand) {
                SetState(fc, true);
            }
        }

        public void Deregister(FingerCollider fc)
        {
            // Instead we wait until you uncurl
            //SetState(fc, false);
        }

        public void SetState(FingerCollider fc, bool state)
        {
            switch (fc.index.finger) {
                default: return;
                case WhichFinger.Thumb: ClampThumb = state; break;
                case WhichFinger.Index: ClampIndex = state; break;
                case WhichFinger.Middle: ClampMiddle = state; break;
                case WhichFinger.Ring: ClampRing = state; break;
                case WhichFinger.Little: ClampLittle = state; break;
            }
        }

        protected override void Start()
        {
            base.Start();

            if (trackRealHand == null)
                trackRealHand = new TrackEvent();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (active && _provider != null) {
                
                if (ClampThumb) {
                    ClampThumb = !ShouldUnClamp(HandBones.ThumbDistal | HandBones.ThumbMiddle | HandBones.ThumbProximal);
                }
                if (ClampIndex) {
                    ClampIndex = !ShouldUnClamp(HandBones.IndexDistal | HandBones.IndexMiddle | HandBones.IndexProximal);
                }
                if (ClampMiddle) {
                    ClampMiddle = !ShouldUnClamp(HandBones.MiddleDistal | HandBones.MiddleMiddle | HandBones.MiddleProximal);
                }
                if (ClampRing) {
                    ClampRing = !ShouldUnClamp(HandBones.RingDistal | HandBones.RingMiddle | HandBones.RingProximal);
                }
                if (ClampLittle) {
                    ClampLittle = !ShouldUnClamp(HandBones.LittleDistal | HandBones.LittleMiddle | HandBones.LittleProximal);
                }

                lastIsAnythingClamped = isAnythingClamped;
                isAnythingClamped = ClampThumb | ClampIndex | ClampMiddle | ClampRing | ClampLittle;

                if (!isAnythingClamped && lastIsAnythingClamped && autoUnlock) {
                    // Free hand
                    locked = false;
                    
                    // TODO do this much cleaner
                    _provider.MaestroHand.ForcedMotorAmplitude = null;
                    _provider.MaestroHand.ForcedVibrationEffect = null;
                    
                    onFree.Invoke();
                } else if (isAnythingClamped && !lastIsAnythingClamped) {
                    // Lock hand
                    locked = true;
                    
                    // TODO do this much cleaner
                    _provider.MaestroHand.ForcedMotorAmplitude = 175;
                    _provider.MaestroHand.ForcedVibrationEffect = null;
                    
                    onLock.Invoke();
                }

                if (locked) {
                    trackRealHand.Invoke(_provider.CurrentPosition);
                    _provider.EnforcePosition(SnapTransform == null ? this.transform : SnapTransform);
                }
            }
        }

        private bool ShouldUnClamp(HandBones bone)
        {
            return (bone & lastClamped) == 0;
        }

        protected override HandBones GetTargetMask()
        {
            HandBones acc = targetedBones;

            if (ClampThumb) acc |= HandBones.ThumbProximal | HandBones.ThumbMiddle | HandBones.ThumbDistal;
            if (ClampIndex) acc |= HandBones.IndexProximal | HandBones.IndexMiddle | HandBones.IndexDistal;
            if (ClampMiddle) acc |= HandBones.MiddleProximal | HandBones.MiddleMiddle | HandBones.MiddleDistal;
            if (ClampRing) acc |= HandBones.RingProximal | HandBones.RingMiddle | HandBones.RingDistal;
            if (ClampLittle) acc |= HandBones.LittleProximal | HandBones.LittleMiddle | HandBones.LittleDistal;

            return acc;
        }
    }
}
