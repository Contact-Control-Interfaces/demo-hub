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
        public bool alwaysLockAll = false;

        public MaestroInteractable interactable;

        public HapticEffect toggleHaptics = new HapticEffect() {Amplitude = 175, Vibration = VibrationEffect.None };

        public void Register(FingerCollider fc)
        {
            if (fc.hpi.whichHand == whichHand) {
                SetState(fc, true);
                if (fc.index.finger != WhichFinger.Palm)
                    LockCorresponding(FromIndex(fc.index));
            }
        }

        public void Deregister(FingerCollider fc)
        {
            // Instead we wait until you uncurl
            //SetState(fc, false);
        }

        public void SetState(FingerCollider fc, bool state)
        {
            if (alwaysLockAll) {
                SetAllStates(state);
            } else {
                switch (fc.index.finger) {
                    default: return;
                    case WhichFinger.Thumb: ClampThumb = state; break;
                    case WhichFinger.Index: ClampIndex = state; break;
                    case WhichFinger.Middle: ClampMiddle = state; break;
                    case WhichFinger.Ring: ClampRing = state; break;
                    case WhichFinger.Little: ClampLittle = state; break;
                }
            }
        }

        public void SetAllStates(bool state)
        {
            ClampIndex = ClampThumb = ClampMiddle = ClampRing = ClampLittle = state;
        }

        public void LockAll()
        {
            SetAllStates(true);
        }

        public void UnlockAll()
        {
            SetAllStates(false);
        }

        protected override void Start()
        {
            base.Start();

            if (trackRealHand == null)
                trackRealHand = new TrackEvent();

            if (interactable == null)
                interactable = GetComponent<MaestroInteractable>();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (active && _provider != null) {
                
                if (ClampThumb) {
                    ClampThumb = !ShouldUnClamp(FromFinger(WhichFinger.Thumb));
                }
                if (ClampIndex) {
                    ClampIndex = !ShouldUnClamp(FromFinger(WhichFinger.Index));
                }
                if (ClampMiddle) {
                    ClampMiddle = !ShouldUnClamp(FromFinger(WhichFinger.Middle));
                }
                if (ClampRing) {
                    ClampRing = !ShouldUnClamp(FromFinger(WhichFinger.Ring));
                }
                if (ClampLittle) {
                    ClampLittle = !ShouldUnClamp(FromFinger(WhichFinger.Little));
                }

                lastIsAnythingClamped = isAnythingClamped;
                isAnythingClamped = ClampThumb | ClampIndex | ClampMiddle | ClampRing | ClampLittle;

                if (!isAnythingClamped && lastIsAnythingClamped && autoUnlock) {
                    // Free hand
                    locked = false;
                    
                    interactable.ResetOverride();
                    
                    onFree.Invoke();
                } else if (isAnythingClamped && !lastIsAnythingClamped) {
                    // Lock hand
                    locked = true;

                    interactable.SetHapticOverride(toggleHaptics);
                    
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

            if (ClampThumb) acc |= FromFinger(WhichFinger.Thumb);
            if (ClampIndex) acc |= FromFinger(WhichFinger.Index);
            if (ClampMiddle) acc |= FromFinger(WhichFinger.Middle);
            if (ClampRing) acc |= FromFinger(WhichFinger.Ring);
            if (ClampLittle) acc |= FromFinger(WhichFinger.Little);

            return acc;
        }
    }
}
