using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Maestro.UI;

namespace Maestro
{
    public class PhysicsGrabManager : IGrabManager
    {
        protected static List<PhysicsGrabManager> AllPhysicsGrabManagers = new List<PhysicsGrabManager>();
        protected IEnumerable<PhysicsGrabManager> OtherPhysicsGrabManagers => AllPhysicsGrabManagers.Except(new[] { this });

        // Keep track of everything being touched, by which fingers
        private Dictionary<MaestroInteractable, TouchingFingers> interactingWith;

        private FingerVisuals[] visuals;
        private GameObject grabAnchor;
        private TouchingFingers grabStartState;

        public override bool Multigrab => false;

        private bool GrabConditionsMet;

        private float timeSinceGrabSatisfied = 0.0f;
        private float timeSinceRelease;

        protected internal float WaitToRelease = 0.15f;
        protected internal float CentroidLerp = 0.0f;

        protected internal float FollowForceVelocityDamper = 0.5f;
        protected internal float FollowForceVelocityScalar = 750;

        protected internal bool debug = false;

        private GameObject centroidObj;
        private Renderer centroidRenderer;

        [Flags]
        public enum TouchingFingers
        {
            ThumbTip = 1,
            IndexTip = 2,
            MiddleTip = 4,
            RingTip = 8,
            LittleTip = 16,
            Palm = 32
        }

        private static MaestroIndex ToIndex(TouchingFingers touching)
        {
            switch (touching) {
                default: throw new NotImplementedException("That finger is not defined!");
                case TouchingFingers.ThumbTip: return new MaestroIndex(WhichFinger.Thumb, PointOnFinger.Tip);
                case TouchingFingers.IndexTip: return new MaestroIndex(WhichFinger.Index, PointOnFinger.Tip);
                case TouchingFingers.MiddleTip: return new MaestroIndex(WhichFinger.Middle, PointOnFinger.Tip);
                case TouchingFingers.RingTip: return new MaestroIndex(WhichFinger.Ring, PointOnFinger.Tip);
                case TouchingFingers.LittleTip: return new MaestroIndex(WhichFinger.Little, PointOnFinger.Tip);
                case TouchingFingers.Palm: return new MaestroIndex(WhichFinger.Palm, PointOnFinger.Base);
            }
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
                case WhichFinger.Palm: return TouchingFingers.Palm;
            }
        }

        private void AddEntry(MaestroInteractable interactable, TouchingFingers newFinger)
        {
            if (interactingWith.ContainsKey(interactable)) {
                interactingWith[interactable] |= newFinger;
            } else {
                interactingWith.Add(interactable, newFinger);
            }

            UpdateVisuals(interactable);
        }

        private void RemoveEntry(MaestroInteractable interactable, TouchingFingers toRemove)
        {
            if (toRemove == 0)
                return;

            if (interactingWith.ContainsKey(interactable)) {
                interactingWith[interactable] &= ~toRemove;
                if (interactingWith[interactable] == 0) {
                    interactingWith.Remove(interactable);
                }
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
                foreach (FingerVisuals pgv in visuals) {
                    pgv.SetMask(interactingWith.ContainsKey(interactable)
                        ? interactingWith[interactable] : 0);
                }
            }
        }

        public struct PhysicsGrabContext : IGrabContext
        {
            bool useGravity;
            Transform parent;
            RigidbodyConstraints constraints;
            float angularDrag;
            bool isKinematic;

            public void Populate(MaestroInteractable interactable)
            {
                useGravity = interactable.rb.useGravity;
                parent = interactable.transform.parent;
                constraints = interactable.rb.constraints;
                isKinematic = interactable.rb.isKinematic;
                angularDrag = interactable.rb.angularDrag;
            }

            public void Apply(MaestroInteractable interactable)
            {
                interactable.rb.useGravity = useGravity;
                interactable.transform.parent = parent;
                interactable.rb.constraints = constraints;
                interactable.rb.isKinematic = isKinematic;
                interactable.rb.angularDrag = angularDrag;
            }
        }

        public PhysicsGrabManager(MaestroContainer mc) : base(mc)
        {
            this.grabType = GrabType.Physics;

            interactingWith = new Dictionary<MaestroInteractable, TouchingFingers>();

            visuals = GameObject.FindObjectsOfType<FingerVisuals>()
                .Where(x => x.which == this.mc.parent.whichHand).ToArray();

            centroidObj = InitCentroid();
            centroidRenderer = centroidObj.GetComponent<Renderer>();

            AllPhysicsGrabManagers.Add(this);
            PhysicsGrabManagerSettings.Apply(this);
        }

        public override void FixedUpdate()
        {
            // gather candidates
            if (!isGrabbing)
                grabCandidates = interactingWith.Keys.Except(grabStates.Select(x => x.target)).ToList();

            base.FixedUpdate();

            // Debug stuff
            if (debug) {
                string debugText = $"{InteractingWithToString()}\n{GrabConditionsMet}\n{timeSinceGrabSatisfied}";

                if (mc.parent.whichHand == WhichHand.RightHand)
                    DisplayBLE.SetRightText(debugText);
                else
                    DisplayBLE.SetLeftText(debugText);
            }
            centroidRenderer.enabled = debug;

            // Update time elapsed vars
            if (!GrabConditionsMet) {
                timeSinceGrabSatisfied += Time.fixedDeltaTime;
            }

            if (!isGrabbing) {
                timeSinceRelease += Time.fixedDeltaTime;
            }

            // Lerp object towards centroid
            if (isGrabbing) {

                RecordHeldObjectPosition(grabAnchor.transform.position);

                Vector3 centroid = GetCentroid(grabStates[0]);
                if (!centroid.Equals(Vector3.negativeInfinity) && !HasNaN(centroid)) {
                    centroidObj.transform.position = centroid;
                    Vector3 temp = Vector3.Lerp(grabAnchor.transform.position, centroid, Time.fixedDeltaTime * CentroidLerp);
                    if (!(temp.Equals(Vector3.negativeInfinity) || grabStates[0].target.maintainPosition))
                        grabAnchor.transform.position = temp;
                }
            }
        }

        private GameObject InitCentroid()
        {
            GameObject result = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            result.transform.localScale = 0.005f * Vector3.one;

            Collider c = result.GetComponent<Collider>();
            c.enabled = false;
            UnityEngine.Object.Destroy(c);

            UnityEngine.Object.Destroy(result.GetComponent<Rigidbody>());

            Renderer r = result.GetComponent<Renderer>();
            ShowOnTop(r, Color.red);

            return result;
        }

        private void ShowOnTop(Renderer r, Color color)
        {
            Shader showAlwaysShader = Shader.Find("GUI/Text Shader");
            if (r != null && showAlwaysShader != null) {
                r.material.shader = showAlwaysShader;
                r.material.color = color;
            }
        }

        private string InteractingWithToString()
        {
            string result = interactingWith.Count.ToString();
            foreach (MaestroInteractable mi in interactingWith.Keys) {
                result += mi.name + ", ";
            }
            return result;
        }

        public override bool CanInitiateGrab(MaestroInteractable toGrab)
        {
            GrabConditionsMet = interactingWith.ContainsKey(toGrab) && GrabbableFingerState(interactingWith[toGrab]);

            return GrabConditionsMet;
        }

        private bool GrabbableFingerState(TouchingFingers touching)
        {
            bool palm = HasPalmTouch(touching);
            bool finger = HasFingerTouch(touching);
            bool thumb = HasThumbTouch(touching);

            return (thumb && finger) || (palm && finger) || (palm && thumb);
        }

        private bool HasPalmTouch(TouchingFingers touching)
        {
            return (touching & TouchingFingers.Palm) > 0;
        }

        private bool HasThumbTouch(TouchingFingers touching)
        {
            return (touching & TouchingFingers.ThumbTip) > 0;
        }

        private bool HasFingerTouch(TouchingFingers touching)
        {
            TouchingFingers anyFinger =
                TouchingFingers.IndexTip |
                TouchingFingers.MiddleTip |
                TouchingFingers.RingTip |
                TouchingFingers.LittleTip;

            return (touching & anyFinger) > 0;
        }

        public override void GrabEnd(GrabState toRelease)
        {
            Debug.Log("Dropping");

            toRelease.context.Apply(toRelease.target);

            grabAnchor.SetActive(false);
            UnityEngine.Object.Destroy(grabAnchor);

            timeSinceRelease = 0.0f;

            toRelease.target.Release();

            toRelease.target.rb.velocity = GetThrowVelocity();
        }

        public override GrabState GrabStart(MaestroInteractable toHold)
        {
            Debug.Log("Grabbing");

            // Force other grabs to end
            var others = OtherPhysicsGrabManagers;
            if (others != null && others.Count() > 0) {
                PhysicsGrabManager toOverride = others.FirstOrDefault(x => x.IsGrabbing(toHold));
                if (toOverride != null) {
                    Debug.Log($"Forcing other grabs to end for {toHold.name}!");
                    toOverride.Relinquish(toHold);
                }
            }

            HeldObjectLastPositions.Clear();

            toHold.Grab();

            // Store current values for when we release
            PhysicsGrabContext oldValues = new PhysicsGrabContext();
            oldValues.Populate(toHold);

            toHold.rb.useGravity = false;
            toHold.rb.isKinematic = false;
            toHold.rb.angularDrag = 100f;

            timeSinceGrabSatisfied = 0.0f;
            GrabConditionsMet = true;

            grabStartState = interactingWith[toHold];

            grabAnchor = generateAnchor(grabTarget.getFollowPoint());
            toHold.transform.parent = grabAnchor.transform;

            return new GrabState(toHold, oldValues);
        }

        public override void OnGrabbing(GrabState grabbed)
        {
            ApplyFollowForce(grabbed);
        }

        public override bool ShouldEndGrab(GrabState grabbed)
        {
            GrabConditionsMet = interactingWith.ContainsKey(grabbed.target)
                && GrabbableFingerState(interactingWith[grabbed.target]);

            if (GrabConditionsMet) {
                timeSinceGrabSatisfied = 0.0f;
            }

            return !GrabConditionsMet && timeSinceGrabSatisfied > WaitToRelease;
        }

        public override void Touch(MaestroInteractable touched, FingerCollider touchedBy)
        {
            if (touched.type != InteractionType.Static)
                AddEntry(touched, FromIndex(touchedBy.index));

            base.Touch(touched, touchedBy);
        }

        public override void UnTouch(MaestroInteractable touched, FingerCollider touchedBy)
        {
            if (touched.type != InteractionType.Static)
                RemoveEntry(touched, FromIndex(touchedBy.index));

            base.UnTouch(touched, touchedBy);
        }

        private Vector3 GetCentroid(GrabState state)
        {
            Vector3 result = Vector3.zero;
            int count = 0;

            TouchingFingers touching = interactingWith.ContainsKey(state.target) ? interactingWith[state.target] : grabStartState;

            var values = (TouchingFingers[]) Enum.GetValues(typeof(TouchingFingers));
            foreach (TouchingFingers flag in values) {
                if (flag == TouchingFingers.Palm)
                    continue;

                if (touching.HasFlag(flag)) {
                    result += mc[ToIndex(flag)].transform.position;
                    count++;
                }
            }

            if (count > 1)
                return result / count;
            else
                return Vector3.negativeInfinity;
        }

        public GameObject generateAnchor(Vector3 position)
        {
            GameObject anchor = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            anchor.transform.SetPositionAndRotation(position, Quaternion.identity);

            anchor.transform.localScale = Vector3.one * 0.01f;
            FixedJoint fj = anchor.AddComponent<FixedJoint>();
            fj.connectedBody = mc[WhichFinger.Index][PointOnFinger.Base].fc.rb;
            fj.massScale = 100;
            fj.connectedMassScale = 100;

            Rigidbody rb = anchor.GetOrMake<Rigidbody>();
            rb.useGravity = false;
            rb.drag = 0;
            rb.mass = 10;
            rb.isKinematic = true;

            anchor.transform.parent = mc.parent.transforms.IndexKnuckle;

            Collider c = anchor.GetComponent<Collider>();
            c.enabled = false;
            UnityEngine.Object.Destroy(c);

            Renderer r = anchor.GetComponent<Renderer>();
            if (debug)
                ShowOnTop(r, Color.blue);
            else {
                r.enabled = false;
                UnityEngine.Object.Destroy(r);
            }

            return anchor;
        }

        private void ApplyFollowForce(GrabState state)
        {
            if (state.target.stayInHand) {
                state.target.gameObject.transform.Translate(grabAnchor.transform.position - state.target.getFollowPoint(), Space.World);

                state.target.rb.angularVelocity = Vector3.zero;
            } else {
                if (!state.target.isTool) {
                    Vector3 dis = grabAnchor.transform.position - state.target.getFollowPoint();

                    if (!HasNaN(dis)) {
                        //dampen
                        Vector3 current = state.target.rb.velocity;
                        current *= (1f - FollowForceVelocityDamper);
                        state.target.rb.velocity = current;

                        Vector3 delta = dis * Mathf.Pow(dis.magnitude * FollowForceVelocityScalar, 2) * Time.fixedDeltaTime; //prev. 500

                        if (!HasNaN(delta))
                            state.target.rb.velocity += delta;
                        else
                            Debug.LogError("HAS NAN!");
                    }
                } else
                    state.target.rb.velocity = Vector3.zero;

                state.target.rb.angularVelocity = Vector3.zero;
            }
        }

        private bool HasNaN(Vector3 dis)
        {
            if (float.IsNaN(dis.x) || float.IsNaN(dis.y) || float.IsNaN(dis.z))
                return true;
            return false;
        }
    }
}
