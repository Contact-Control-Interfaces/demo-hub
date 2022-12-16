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
        private Dictionary<MaestroInteractable, List<MaestroIndex>> interactingWith;

        //private FingerVisuals[] visuals;
        private GameObject grabAnchor;
        private List<MaestroIndex> grabStartState;

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

        private static WhichFinger[] fingers = new WhichFinger[] { WhichFinger.Index, WhichFinger.Middle, WhichFinger.Ring, WhichFinger.Little };

        private void AddEntry(MaestroInteractable interactable, MaestroIndex newFinger)
        {
            if (!interactingWith.ContainsKey(interactable)) {
                interactingWith.Add(interactable, new List<MaestroIndex>());
            }

            interactingWith[interactable].Add(newFinger);
        }

        private void RemoveEntry(MaestroInteractable interactable, MaestroIndex toRemove)
        {
            if (interactingWith.ContainsKey(interactable)) {
                interactingWith[interactable].Remove(toRemove);
                if (interactingWith[interactable].Count == 0) {
                    interactingWith.Remove(interactable);
                }
            }
        }

        private void SetEntry(MaestroInteractable interactable, MaestroIndex context)
        {
            List<MaestroIndex> temp = new List<MaestroIndex>();
            temp.Add(context);

            if (interactingWith.ContainsKey(interactable)) {
                interactingWith[interactable] = temp;
            } else {
                interactingWith.Add(interactable, temp);
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

            interactingWith = new Dictionary<MaestroInteractable, List<MaestroIndex>>();

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

        private bool GrabbableFingerState(List<MaestroIndex> touching)
        {
            bool palm = HasPalmTouch(touching);
            bool finger = HasFingerTouch(touching);
            bool thumb = HasThumbTouch(touching);

            return (thumb && finger) || (palm && thumb) || (!mc.parent.isFlat && palm && finger);
        }

        private bool HasPalmTouch(List<MaestroIndex> touching)
        {
            return touching.Any(x => x.finger == WhichFinger.Palm);
        }

        private bool HasThumbTouch(List<MaestroIndex> touching)
        {
            return touching.Any(x => x.finger == WhichFinger.Thumb);
        }

        private bool HasFingerTouch(List<MaestroIndex> touching)
        {
            return touching.Any(x => IsFinger(x));
        }

        private bool IsFinger(MaestroIndex index)
        {
            return Array.IndexOf(fingers, index.finger) >= 0;
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

            return !DisallowDropping && (!GrabConditionsMet && timeSinceGrabSatisfied > WaitToRelease);
        }

        public override void Touch(MaestroInteractable touched, FingerCollider touchedBy)
        {
            if (touched.type != InteractionType.Static)
                AddEntry(touched, touchedBy.index);

            base.Touch(touched, touchedBy);
        }

        public override void UnTouch(MaestroInteractable touched, FingerCollider touchedBy)
        {
            if (touched.type != InteractionType.Static)
                RemoveEntry(touched, touchedBy.index);

            base.UnTouch(touched, touchedBy);
        }

        private Vector3 GetCentroid(GrabState state)
        {
            Vector3 result = Vector3.zero;
            int count = 0;

            List<MaestroIndex> touching = interactingWith.ContainsKey(state.target) ? interactingWith[state.target] : grabStartState;

            foreach(MaestroIndex index in touching) {
                if (index.finger == WhichFinger.Palm) {
                    result += mc.parent.Palm.position;
                } else {
                    result += mc[index].transform.position;
                }
                count++;
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
