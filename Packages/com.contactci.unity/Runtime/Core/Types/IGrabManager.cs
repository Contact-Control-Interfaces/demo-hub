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
        Arcade, Physics, None
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

        protected List<Vector3> HeldObjectLastPositions = new List<Vector3>();
        protected int HistoryCount = 10;

        public bool DisallowDropping;

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
         * Is this manager currently holding this object?
         */
        protected bool IsGrabbing(MaestroInteractable toCheck)
        {
            return grabStates.Any(x => x.target == toCheck);
        }

        /**
         * Force this grab manager to drop a given held object
         */
        public virtual bool Relinquish(MaestroInteractable toRelinquish)
        {
            if (grabStates.Count == 0)
                return false; // Nothing to drop

            var toEnd = grabStates.Where(x => x.target == toRelinquish).ToList();
            if (toEnd.Count == 0)
                return false; // No matching states found

            foreach (GrabState state in toEnd) {
                GrabEnd(state);
                grabStates.Remove(state);
            }
            return true; // All grabs ended
        }

        /**
         * Called each FixedUpdate
         */
        public virtual void FixedUpdate()
        {
            // End all grabs that we can
            List<GrabState> toRemove = new List<GrabState>();
            foreach (GrabState gs in grabStates) {
                if (Inactive(gs.target)) {
                    toRemove.Add(gs);
                } else if (ShouldEndGrab(gs)) {
                    GrabEnd(gs);
                    toRemove.Add(gs);
                }
            }
            foreach (GrabState gs in toRemove) {
                grabStates.Remove(gs);
            }

            // Prune grab candidates
            grabCandidates.RemoveAll(x => Inactive(x));

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

        protected virtual void RecordHeldObjectPosition(Vector3 position)
        {
            HeldObjectLastPositions.Add(position);
            while (HeldObjectLastPositions.Count > HistoryCount)
                HeldObjectLastPositions.RemoveAt(0);
        }

        protected virtual Vector3 GetThrowVelocity()
        {
            if (HeldObjectLastPositions.Count <= 1)
                return Vector3.zero;

            List<Vector3> velocities = new List<Vector3>();
            for (int i = 1; i < HeldObjectLastPositions.Count; i++) {
                velocities.Add(HeldObjectLastPositions[i] - HeldObjectLastPositions[i - 1]);
            }

            return velocities.Aggregate(Vector3.zero, (acc, next) => acc + next) / (velocities.Count * Time.fixedDeltaTime);
        }

        protected virtual bool Inactive(MaestroInteractable interactable)
        {
            return interactable == null || !interactable.gameObject.activeInHierarchy;
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
