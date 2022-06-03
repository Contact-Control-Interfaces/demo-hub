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
        public IMaestroHand hpi { get; protected set; } // the parent hand's interaction script.

        public Rigidbody rb; // my rigidbody

        public MaestroInteractable touching; // the thing I'm touching currently

        public MaestroInteractable lastTouching;

        public Renderer rend; // the debug renderer for the collider. Is disabled by default.

        private PointOnHand parent;

        public MaestroIndex index { get { return parent.index; } } // which finger am I?

        public bool Contacting {
            get {
                return AllTouching.Count + DefaultTouching.Count > 0;
            }
        }

        public AudioSource source;

        public Vector3 lastLocation;

        #region Hand position helpers
        public bool isTip { 
            get {
                return isFinger && index.point == PointOnFinger.Tip;
            } 
        }

        public bool isMiddleJoint {
            get {
                return isFinger && index.point == PointOnFinger.Middle;
            }
        }

        public bool isFingerBase {
            get {
                return isFinger && index.point == PointOnFinger.Base;
            }
        }

        public bool isPalm {
            get {
                return index.finger == WhichFinger.Palm;
            }
        }

        public bool isFinger {
            get {
                return !isPalm;
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

        public void SetParent(PointOnHand poh, IMaestroHand hand)
        {
            SetParentPOH(poh);
            SetParentHPI(hand);
        }

        public void SetParentHPI(IMaestroHand hand)
        {
            this.hpi = hand;
        }

        public void SetParentPOH(PointOnHand poh)
        {
            this.parent = poh;
        }

        #region Mono Behaviours
        void Awake()
        {
            netImpulse = Vector3.zero;
            rb = GetComponent<Rigidbody>();
            if (!rb) rb = this.gameObject.AddComponent<Rigidbody>();

            rend = GetComponent<Renderer>();

            lastLocation = this.transform.position;
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

                if (ints.Count > 0)
                    touching = ints.Max;
                else
                    Debug.LogWarning("No interactable for colliders!");
            } else {
                touching = null;
            }

            if (rend != null) {
                rend.enabled = hpi.ShowOnlyWhileTouching ? this.Contacting : true;
            }

            if (rb)
                lastLocation = this.rb.position;
            else
                lastLocation = this.transform.position;

            // Keep track of the last thing we've touched, other than current
            if (touching) {
                lastTouching = touching;
            }
        }

        private void OnDisable()
        {
            AllTouching.Clear();
            DefaultTouching.Clear();
        }
        #endregion

        #region On Collision
        void OnCollisionEnter(Collision c)
        {
            if (TryGetInteractable(c.collider, out MaestroInteractable interactable))
            {
                if (!AllTouching.Contains(c.collider))
                    AllTouching.Add(c.collider);
                hpi.grabManager.Touch(interactable, this);

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
            DefaultTouching.Remove(c.collider);

            netImpulse += c.impulse;
        }

        private void OnCollisionStay(Collision c)
        {
            if (TryGetInteractable(c.collider, out MaestroInteractable interactable))
            {
                if (AllTouching.Contains(c.collider))
                    interactable.WhileTouch(this);
            }
        }
        #endregion

        public void AddAudioSource()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.volume = 0.5f;
            source.clip = Resources.Load<AudioClip>("Sounds/tap");
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