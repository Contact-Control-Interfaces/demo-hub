using Maestro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Maestro
{
    public struct ArcadeGrabContext : IGrabContext
    {
        bool useGravity;
        Transform parent;
        RigidbodyConstraints constraints;

        public void Populate(MaestroInteractable interactable)
        {
            useGravity = interactable.rb.useGravity;
            parent = interactable.transform.parent;
            constraints = interactable.rb.constraints;
        }

        public void Apply(MaestroInteractable interactable)
        {
            interactable.rb.useGravity = useGravity;
            interactable.transform.parent = parent;
            interactable.rb.constraints = constraints;
        }
    }

    public class ArcadeGrabManager : IGrabManager
    {
        static List<ArcadeGrabManager> allManagers;
        static bool overrideOtherGrabs = true;

        private List<MaestroInteractable> endedThisFrame;
        private Dictionary<MaestroInteractable, float> dontGrab;
        static float dontGrabDuration = 0.125f;

        static ArcadeGrabManager()
        {
            allManagers = new List<ArcadeGrabManager>();
        }

        public static void Register(ArcadeGrabManager agm)
        {
            allManagers.Add(agm);
        }

        static bool IsAlreadyGrabbedAnywhere(MaestroInteractable mi)
        {
            return allManagers.Any(x => x.IsAlreadyGrabbed(mi));
        }

        public struct FingerCurls
        {
            float Thumb, Index, Middle, Ring, Little;

            public float this[WhichFinger finger] {
                get {
                    switch (finger) {
                        default: return 0f;
                        case WhichFinger.Thumb: return Thumb;
                        case WhichFinger.Index: return Index;
                        case WhichFinger.Middle: return Middle;
                        case WhichFinger.Ring: return Ring;
                        case WhichFinger.Little: return Little;
                    }
                }

                set {
                    switch (finger) {
                        case WhichFinger.Thumb: Thumb = value; break;
                        case WhichFinger.Index: Index = value; break;
                        case WhichFinger.Middle: Middle = value; break;
                        case WhichFinger.Ring: Ring = value; break;
                        case WhichFinger.Little: Little = value; break;
                    }
                }
            }

            public float FingerAverage {
                get { return (Index + Middle + Ring + Little) / 4; }
            }
        };

        private FingerCurls fingerCurls;

        private NearbyObjects nearbyObjects;

        private bool lastCanInitiateAnyGrab = false;

        public float GrabThreshold { get; set; }

        public ArcadeGrabManager(MaestroContainer mc) : base(mc)
        {
            GrabThreshold = 0.35f; //in case we want to have this dynamically change later on
            grabType = GrabType.Arcade;
            fingerCurls = new FingerCurls();
            grabStates = new List<GrabState>();

            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            temp.transform.parent = mc.PalmBase.transform;
            nearbyObjects = temp.AddComponent<NearbyObjects>();
            nearbyObjects.transform.localPosition = Vector3.zero;

            dontGrab = new Dictionary<MaestroInteractable, float>();
            endedThisFrame = new List<MaestroInteractable>();

            Register(this);
        }

        public override void FixedUpdate()
        {
            lastCanInitiateAnyGrab = this.CanInitiateGrab();
            CalculateFingerCurls();

            nearbyObjects.transform.localPosition = mc.PalmBase.transform.InverseTransformPoint(mc[WhichFinger.Middle].Base.transform.position);

            GatherCandidates();

            base.FixedUpdate();

            // Expire objects we're ignoring
            MaestroInteractable[] canGrabAgain = dontGrab.Where(x => Time.time - x.Value > dontGrabDuration).Select(x => x.Key).ToArray();
            foreach (MaestroInteractable mi in canGrabAgain) {
                dontGrab.Remove(mi);
            }

            // Start ignoring objects that we just stopped interacting with
            foreach (MaestroInteractable mi in endedThisFrame) {
                if (!grabStates.Any(x => x.target == mi)) {
                    // grab ended, no other grab started, so disallow grabs for a bit
                    dontGrab.Add(mi, Time.fixedTime);
                }
            }

            endedThisFrame.Clear();
        }

        private void GatherCandidates()
        {
            List<MaestroInteractable> candidates = nearbyObjects.get();

            // Don't grab static things
            candidates.RemoveAll(x => x.type == InteractionType.Static);

            // Don't grab objects we're already grabbing with this hand
            foreach (GrabState gs in grabStates) {
                candidates.RemoveAll(x => x == gs.target);
            }

            // Don't grab objects that we just dropped
            foreach (MaestroInteractable dg in dontGrab.Keys) {
                candidates.Remove(dg);
            }
            grabCandidates = candidates;
        }

        private void CalculateFingerCurl(WhichFinger whichFinger)
        {
            if (whichFinger == WhichFinger.Palm)
                return;
            FingerContainer finger = mc[whichFinger];
            Quaternion tipRot = finger.Tip.transform.rotation;
            Quaternion middleRot = finger.Middle.transform.rotation;
            Quaternion baseRot = finger.Base.transform.rotation;

            float angle = Quaternion.Angle(tipRot, baseRot);
            fingerCurls[finger.whichFinger] = angle / 180;
        }

        private void CalculateFingerCurls()
        {
            foreach (WhichFinger whichFinger in (WhichFinger[])Enum.GetValues(typeof(WhichFinger))) {
                CalculateFingerCurl(whichFinger);
            }
        }

        private bool CanInitiateGrab()
        {
            return fingerCurls.FingerAverage > GrabThreshold;
        }

        private bool IsAlreadyGrabbed(MaestroInteractable alreadyGrabbed)
        {
            return grabStates.Any(x => x.target == alreadyGrabbed);
        }

        private void EndOtherGrabs(MaestroInteractable toGrab)
        {
            var fromManagers = ArcadeGrabManager.allManagers.Where(x => x.IsAlreadyGrabbed(toGrab)).ToArray();

            foreach (ArcadeGrabManager agm in fromManagers) {
                GrabState[] toEnd = agm.grabStates.Where(x => x.target == toGrab).ToArray();
                foreach (GrabState state in toEnd) {
                    agm.GrabEnd(state);
                }
            }
        }

        public override bool CanInitiateGrab(MaestroInteractable toGrab)
        {
            bool canGrab = CanInitiateGrab() && !lastCanInitiateAnyGrab;
            if (canGrab) {
                bool otherGrab = IsAlreadyGrabbedAnywhere(toGrab);
                if (otherGrab && overrideOtherGrabs) {
                    // override
                    EndOtherGrabs(toGrab);
                    return true;
                } else if (otherGrab) {
                    // Object already grabbed, no override
                    return false;
                }
            }
            return canGrab;
        }

        public override bool ShouldEndGrab(GrabState grabbed)
        {
            return !CanInitiateGrab() || grabbed.Remove;
        }

        public override GrabState GrabStart(MaestroInteractable toHold)
        {
            // Store current values for when we release
            ArcadeGrabContext oldValues = new ArcadeGrabContext();
            oldValues.Populate(toHold);

            toHold.transform.parent = mc.PalmBase.transform;
            toHold.rb.useGravity = false;
            toHold.rb.velocity = Vector3.zero;
            toHold.rb.angularVelocity = Vector3.zero;
            toHold.rb.constraints = toHold.rb.constraints | RigidbodyConstraints.FreezeAll;

            return new GrabState(toHold, oldValues);
        }

        public override void GrabEnd(GrabState toRelease)
        {
            MaestroInteractable target = toRelease.target;

            // Reset values and release object
            toRelease.context.Apply(target);

            // Remove all grab states for this object
            this.grabStates.Where(x => x.target == target).ToList()
                .ForEach(x => x.MarkForRemoval());

            endedThisFrame.Add(target);
        }

        public override void OnGrabbing(GrabState grabbed)
        {
            //NOTHING
        }
    }
}
