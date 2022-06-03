using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public class PhysicsGrabManager : IGrabManager
    {
        // Keep track of everything being touched, by which fingers
        private Dictionary<MaestroInteractable, TouchingFingers> interactingWith;

        private FingerVisuals[] visuals;

        [Flags]
        public enum TouchingFingers
        {
            ThumbTip = 1,
            IndexTip = 2,
            MiddleTip = 4,
            RingTip = 8,
            LittleTip = 16
        }

        private static TouchingFingers FromIndex(MaestroIndex index)
        {
            switch (index.finger) {
                default: throw new NotImplementedException("That finger is not defined!");
                case WhichFinger.Thumb: return TouchingFingers.ThumbTip;
                case WhichFinger.Index: return TouchingFingers.IndexTip;
                case WhichFinger.Middle: return TouchingFingers.MiddleTip;
                case WhichFinger.Ring: return TouchingFingers.RingTip;
                case WhichFinger.Little: return TouchingFingers.LittleTip;
                case WhichFinger.Palm: return 0;
            }
        }

        private void UpdateEntry(MaestroInteractable interactable, TouchingFingers newFinger)
        {
            if (interactingWith.ContainsKey(interactable)) {
                interactingWith[interactable] |= newFinger;
            } else {
                interactingWith.Add(interactable, newFinger);
            }

            UpdateVisuals(interactable);
        }

        private void SetEntry(MaestroInteractable interactable, TouchingFingers context)
        {
            if (interactingWith.ContainsKey(interactable)) {
                interactingWith[interactable] = context;
            } else {
                interactingWith.Add(interactable, context);
            }

            UpdateVisuals(interactable);
        }

        private void UpdateVisuals(MaestroInteractable interactable)
        {
            if (visuals != null && visuals.Length > 0) {
                foreach (FingerVisuals pgv in visuals)
                    pgv.SetMask(interactingWith[interactable]);
            }
        }

        public PhysicsGrabManager(MaestroContainer mc) : base(mc)
        {
            interactingWith = new Dictionary<MaestroInteractable, TouchingFingers>();
            visuals = GameObject.FindObjectsOfType<FingerVisuals>();
        }

        public override void FixedUpdate()
        {
            // gather candidates

            base.FixedUpdate();
        }

        public override bool CanInitiateGrab(MaestroInteractable toGrab)
        {
            return false;
        }

        public override void GrabEnd(GrabState toRelease)
        {

        }

        public override GrabState GrabStart(MaestroInteractable toHold)
        {
            return new GrabState();
        }

        public override void OnGrabbing(GrabState grabbed)
        {
            
        }

        public override bool ShouldEndGrab(GrabState grabbed)
        {
            return true;
        }

        public override void Touch(MaestroInteractable touched, FingerCollider touchedBy)
        {
            UpdateEntry(touched, FromIndex(touchedBy.index));

            base.Touch(touched, touchedBy);
        }
    }
}
