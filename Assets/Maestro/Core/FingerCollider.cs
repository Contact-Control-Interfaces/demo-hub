//=============================================================================
//
// Purpose: Keeps track of what this fingertip is touching. This script is added automatically; do not place it in your scene.
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Maestro
{
    public enum HAND_POSITION
    {
        ThumbTip,       IndexTip,       MiddleTip,      RingTip,        LittleTip,  
        ThumbMiddle,    IndexMiddle,    MiddleMiddle,   RingMiddle,     LittleMiddle,
        ThumbBase,      IndexBase,      MiddleBase,     RingBase,       LittleBase,
        PalmBase
    };

    public class FingerCollider : MonoBehaviour
    {
        public IMaestroHand hpi; // the parent hand's interaction script.

        public Rigidbody rb; // my rigidbody

        public MaestroInteractable touching; // the thing I'm touching currently

        public MaestroInteractable lastTouching;

        public Renderer rend; // the debug renderer for the collider. Is disabled by default.

        public Vector3 colnrm; // the normal of the current collision

        private PointOnHand parent;

        public MaestroIndex index { get { return parent.index; } } // which finger am I?

        [HideInInspector]
        public Collider col; //keep track of my collider so I can turn it off in certain situations

        public bool TriggerTouching = true;

        public bool Contacting {
            get {
                return AllTouching.Count + DefaultTouching.Count > 0;
            }
        }

        public AudioSource source;

        public Vector3 lastLocation;

        private Color _paintColor = Color.clear;
        public Color PaintColor {
            get { return _paintColor; }
            set {
                _paintColor = value;
                if (rend && rend.material)
                    rend.material.color = _paintColor == Color.clear ? Color.clear : _paintColor;
            }
        }

        #region Hand position helpers
        public bool isTip { 
            get {
                return index.point == PointOnFinger.Tip;
            } 
        }

        public bool isMiddleJoint {
            get {
                return index.point == PointOnFinger.Middle;
            }
        }

        public bool isFingerBase {
            get {
                return index.point == PointOnFinger.Base
                    && !isPalmBase;
            }
        }

        public bool isPalmBase {
            get {
                return index.finger == WhichFinger.Palm;
            }
        }

        public bool isThumbFinger {
            get {
                return index.finger == WhichFinger.Thumb;
            }
        }

        public bool isIndexFinger {
            get {
                return index.finger == WhichFinger.Index;
            }
        }

        public bool isMiddleFinger {
            get {
                return index.finger == WhichFinger.Middle;
            }
        }

        public bool isRingFinger {
            get {
                return index.finger == WhichFinger.Ring;
            }
        }

        public bool isLittleFinger {
            get {
                return index.finger == WhichFinger.Little;
            }
        }
        #endregion

        private Vector3 netImpulse;
        public Vector3 NetImpulse {
            get {
                Vector3 temp = netImpulse;
                netImpulse = Vector3.zero;
                return temp;
            }
        }

        private List<Collider> AllTouching = new List<Collider>();
        private List<Collider> DefaultTouching = new List<Collider>();
        private Dictionary<Collider, MaestroInteractable> mapper = new Dictionary<Collider, MaestroInteractable>();

        public void SetParent(PointOnHand poh)
        {
            this.parent = poh;
        }

        #region Mono Behaviours
        void Awake()
        {
            netImpulse = Vector3.zero;
            rb = GetComponent<Rigidbody>();

            lastLocation = this.transform.position;

            TriggerTouching = false;
        }

        void Update()
        {
            //update the public touching variable
            AllTouching.RemoveAll(x => x == null);
            DefaultTouching.RemoveAll(x => x == null);
            if (AllTouching.Count > 0) {
                SortedSet<MaestroInteractable> ints;

                switch (hpi.interactionPriority) {
                    default:
                    case InteractionPriority.PrioritizeAmplitude:
                        ints = new SortedSet<MaestroInteractable>(AllTouching.Select(x => mapper[x]), new PrioritizeAmplitude());
                        break;
                    case InteractionPriority.PrioritizeVibrationEffect:
                        ints = new SortedSet<MaestroInteractable>(AllTouching.Select(x => mapper[x]), new PrioritizeVibrationEffect());
                        break;
                }

                //TODO sort this
                /*
                SortedSet<MaestroInteractable> ints = new SortedSet<MaestroInteractable>();
                foreach (Collider c in AllTouching) {
                    MaestroInteractable temp = null;
                    if (!mapper.TryGetValue(c, out temp)) {
                        Debug.LogWarning(string.Format("Collider on {0} unaccounted for!", c.gameObject.name));
                    } else {
                        ints.Add(temp);
                    }
                }*/

                if (ints.Count > 1) {
                    //Debug.Log(string.Format("Detected {0} different interactables, need to decide", ints.Count));
                }

                if (ints.Count > 0)
                    touching = ints.Max;
                else
                    Debug.LogWarning("No interactable for colliders!");
            } else {
                touching = null;
            }


            if (rb)
                lastLocation = this.rb.position;
            else
                lastLocation = this.transform.position;

            //this mess just resets the finger clamping after 0.2 seconds of not touching anything
            if (touching) {
                lastTouching = touching;
            }
        }

        private void OnDisable()
        {
            //rend.enabled = false;
            AllTouching.Clear();
            DefaultTouching.Clear();
        }
        #endregion

        #region On Collision
        void OnCollisionEnter(Collision c)
        {
            if (TryGetInteractable(c.collider, out MaestroInteractable interactable))
            {
                AllTouching.Add(c.collider);
                interactable.Touch(this);
                if (!interactable.IgnoreTaps)
                {
                    float scale = 1.50f;
                    float helper = Mathf.Max(0.20f, Mathf.Min(1.0f, this.rb.velocity.magnitude * scale));

                    if (source != null && interactable.type == InteractionType.Static)
                    {
                        source.volume = helper;
                        source.Play();
                    }
                }
            }
            else if (c.collider.gameObject.GetComponent<FingerCollider>() == null && c.collider.gameObject.GetComponentInParent<MaestroContainer>() == null)
            {
                DefaultTouching.Add(c.collider);
            }
        }

        void OnCollisionExit(Collision c)
        {
            if (AllTouching.Remove(c.collider))
            {
                if (mapper.TryGetValue(c.collider, out MaestroInteractable interactable))
                {
                    interactable.Untouch(this);
                }
                else
                {
                    Debug.LogWarning("Removed collider without mapping!");
                }
            }
            else
                DefaultTouching.Remove(c.collider);

            netImpulse += c.impulse;
        }
        #endregion

        public void AddAudioSource()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.volume = 0.5f;
            source.clip = Resources.Load<AudioClip>("Sounds/tap");
        }

        public void SetPhysicMaterial(PhysicMaterial material)
        {
            this.col.material = material;
        }

        // Creates the actual colliders for fingertips/palm
        public void makeRend(float radius)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            col = g.GetComponent<Collider>();

            g.transform.parent = transform;
            g.transform.localPosition = Vector3.zero;
            g.transform.localRotation = Quaternion.identity;
            g.transform.localScale = radius * Vector3.one * 2;
            g.GetComponent<Renderer>().material = (Material)Resources.Load("ContactAccent", typeof(Material));
            rend = g.GetComponent<Renderer>();



            //Ignore all collisions with the hand itself
            if (hpi != null) {
                foreach (Collider c in hpi.GetComponentsInChildren<Collider>()) {
                    //Physics.IgnoreCollision(releaseCol, c);
                    Physics.IgnoreCollision(col, c);
                }
            }
        }

        public void makeRend(Vector3 size)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            col = g.GetComponent<Collider>();
            g.transform.parent = transform;
            g.transform.localPosition = Vector3.zero;
            g.transform.localRotation = Quaternion.identity;
            g.transform.localScale = size;
            g.GetComponent<Renderer>().material = (Material)Resources.Load("colliderDebug", typeof(Material));
            rend = g.GetComponent<Renderer>();



            if (hpi != null) {
                foreach (Collider c in hpi.GetComponentsInChildren<Collider>()) {
                    Physics.IgnoreCollision(col, c);
                    //Physics.IgnoreCollision(releaseCol, c);
                }
            }
        }

        //Check whether a collider is Maestro Interactable
        bool TryGetInteractable(Collider c, out MaestroInteractable parentInteractable)
        {
            parentInteractable = null;

            if (c == null || c.attachedRigidbody == null)
                return false;

            // Try to retrieve saved interactable pairing
            if (!mapper.TryGetValue(c, out parentInteractable)) {
                parentInteractable = c.attachedRigidbody.GetComponentInParent<MaestroInteractable>();

                // Update map if interactable found
                if (parentInteractable != null)
                    mapper.Add(c, parentInteractable);
            }

            return parentInteractable != null && c.GetComponentInParent<FingerCollider>() == null;
        }
    }
}