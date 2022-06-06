using Maestro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Maestro
{
    public enum GrabType
    {
        Arcade
    };

    public struct GrabState
    {
        public MaestroInteractable target;

        public IGrabContext context;

        private bool toBeRemoved;
        public bool Remove { get { return toBeRemoved; } }

        public GrabState(MaestroInteractable target, IGrabContext context)
        {
            this.target = target;
            this.context = context;
            toBeRemoved = false;
        }

        public void MarkForRemoval()
        {
            toBeRemoved = true;
        }
    };

    public abstract class IGrabManager
    {
        protected MaestroContainer mc { get; set; }

        public GrabType grabType { get; protected set; }

        /**
         * Can we grab multiple things at once?
         */
        public virtual bool Multigrab { get { return true; } }

        public virtual bool isGrabbing { get { return grabStates.Count > 0; } }

        public bool wasGrabbing { get; protected set; }

        public List<GrabState> grabStates { get; protected set; }

        public List<MaestroInteractable> grabCandidates { get; protected set; }

        public virtual MaestroInteractable grabTarget { get { return grabCandidates.Count > 0 ? grabCandidates[0] : null; } }

        protected IGrabManager(MaestroContainer mc)
        {
            this.mc = mc;
            wasGrabbing = false;

            grabStates = new List<GrabState>();
            grabCandidates = new List<MaestroInteractable>();
        }

        /**
         * Can a grab be initiated right now?
         */
        public abstract bool CanInitiateGrab(MaestroInteractable toGrab);

        public bool CanInitiateAnyGrab()
        {
            foreach (MaestroInteractable mi in grabCandidates) {
                if (CanInitiateGrab(mi)) {
                    return true;
                }
            }
            return false;
        }

        /**
         * Should we stop grabbing the object?
         */
        public abstract bool ShouldEndGrab(GrabState grabbed);

        /**
         * Start a grab with this object
         */
        public abstract GrabState GrabStart(MaestroInteractable toHold);

        /**
         * End a grab with this object
         */
        public abstract void GrabEnd(GrabState toRelease);

        /**
         * Called each frame the object is held
         */
        public abstract void OnGrabbing(GrabState grabbed);

        public virtual void OnGrabbing()
        {
            foreach (GrabState gs in grabStates) {
                OnGrabbing(gs);
            }
        }

        /**
         * Called each FixedUpdate
         */
        public virtual void FixedUpdate()
        {
            // End all grabs that we can
            List<GrabState> toRemove = new List<GrabState>();
            foreach (GrabState gs in grabStates) {
                if (ShouldEndGrab(gs)) {
                    GrabEnd(gs);
                    toRemove.Add(gs);
                }
            }
            foreach (GrabState gs in toRemove) {
                grabStates.Remove(gs);
            }

            // Start all grabs that we can
            if (!isGrabbing || Multigrab) {
                foreach (MaestroInteractable mi in grabCandidates) {
                    if (CanInitiateGrab(mi)) {
                        grabStates.Add(GrabStart(mi));

                        if (!Multigrab)
                            break;
                    }
                }
            }

            // Call OnGrabbing 
            if (isGrabbing)
                OnGrabbing();

            wasGrabbing = isGrabbing;
        }

        public virtual void Touch(MaestroInteractable touched, FingerCollider touchedBy)
        {
            touched.Touch(touchedBy);
        }

        public virtual void UnTouch(MaestroInteractable touched, FingerCollider touchedBy)
        {
            touched.Untouch(touchedBy);
        }
    }    
}
