using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Maestro
{
    public class MaestroContainer : MonoBehaviour, IEnumerable, IEnumerable<PointOnHand>
    {
        /* TODO should be replaced with a IMaestroHand */
        [HideInInspector]
        public MaestroHand parent;

        private Dictionary<WhichFinger, FingerContainer> fingers;

        public Transform PalmContainer { get; internal set; }

        public int FingerCount { get { return 5; } }
        public int PointOnHandCount { get { return FingerCount * 3; } }

        void Awake()
        {
            fingers = new Dictionary<WhichFinger, FingerContainer>();
        }

        public List<FingerContainer> GetFingers()
        {
            return fingers.Values.Where(x => x.whichFinger != WhichFinger.Palm).ToList();
        }

        #region Indexing
        public FingerContainer this[WhichFinger finger] {
            get => fingers[finger];

            set {
                if (!fingers.ContainsKey(finger)) {
                    fingers.Add(finger, value);
                } else {
                    fingers[finger] = value;
                }
                fingers[finger].Parent = this;
                fingers[finger].whichFinger = finger;
            }
        }

        public PointOnHand this[MaestroIndex index] => fingers[index.finger][index.point];
        #endregion

        #region IEnumerable impl
        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable<PointOnHand>)this).GetEnumerator();
        }

        IEnumerator<PointOnHand> IEnumerable<PointOnHand>.GetEnumerator()
        {
            foreach (PointOnHand poh in fingers[WhichFinger.Thumb]) {
                yield return poh;
            }
            foreach (PointOnHand poh in fingers[WhichFinger.Index]) {
                yield return poh;
            }
            foreach (PointOnHand poh in fingers[WhichFinger.Middle]) {
                yield return poh;
            }
            foreach (PointOnHand poh in fingers[WhichFinger.Ring]) {
                yield return poh;
            }
            foreach (PointOnHand poh in fingers[WhichFinger.Little]) {
                yield return poh;
            }
        }
        #endregion
    }

    public class FingerContainer : IEnumerable, IEnumerable<PointOnHand>
    {
        public WhichFinger whichFinger { get; internal set; }

        public MaestroContainer Parent { get; internal set; }

        public PointOnHand Tip { get; private set; }
        public PointOnHand Distal { get; private set; }
        public PointOnHand Middle { get; private set; }
        public PointOnHand Base { get; private set; }

        public PointOnHand DistalSegment { get; private set; }
        public PointOnHand Proximal { get; private set; }
        public PointOnHand Metacarpal { get; private set; }

        public FingerContainer(PointOnHand tip, PointOnHand middle, PointOnHand fingerBase, PointOnHand distalSegment, PointOnHand proximal, PointOnHand metacarpal, PointOnHand distal = null)
        {
            this.Tip = tip;
            this.Tip.whereOnFinger = PointOnFinger.Tip;
            this.Tip.parent = this;

            if (distal != null) {
                this.Distal = distal;
                this.Distal.whereOnFinger = PointOnFinger.Distal;
                this.Distal.parent = this;
            }

            this.Middle = middle;
            this.Middle.whereOnFinger = PointOnFinger.Middle;
            this.Middle.parent = this;

            this.Base = fingerBase;
            this.Base.whereOnFinger = PointOnFinger.Base;
            this.Base.parent = this;

            this.DistalSegment = distalSegment;
            this.DistalSegment.whereOnFinger = PointOnFinger.DistalDigit;
            this.DistalSegment.parent = this;

            this.Proximal = proximal;
            this.Proximal.whereOnFinger = PointOnFinger.ProximalDigit;
            this.Proximal.parent = this;

            this.Metacarpal = metacarpal;
            // Metacarpals unused because they are covered by the palm
            // once PointOnFinger has a Metacarpal value we can uncomment this
            //this.Metacarpal.whereOnFinger = PointOnFinger.Metacarpal;
            this.Metacarpal.parent = this;
        }

        public PointOnHand this[PointOnFinger pof] {
            get {
                switch (pof) {
                    case PointOnFinger.Tip: return Tip;
                    case PointOnFinger.Distal: return Distal;
                    case PointOnFinger.Middle: return Middle;
                    case PointOnFinger.Base: return Base;
                    case PointOnFinger.DistalDigit: return DistalSegment;
                    case PointOnFinger.ProximalDigit: return Proximal;
                    default: return null;
                }
            }
        }

        #region IEnumerable impl
        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable<PointOnHand>)this).GetEnumerator();
        }

        IEnumerator<PointOnHand> IEnumerable<PointOnHand>.GetEnumerator()
        {
            yield return Tip;
            yield return DistalSegment;
            yield return Middle;
            yield return Proximal;
            yield return Base;
        }
        #endregion
    }

    public class PointOnHand
    {
        public PointOnFinger whereOnFinger { get; internal set; }

        public WhichFinger whichFinger { 
            get { 
                return parent != null ? parent.whichFinger : WhichFinger.Palm; 
            } 
        }
         
        public MaestroIndex index { 
            get {
                return new MaestroIndex(parent != null ? parent.whichFinger : WhichFinger.Palm, whereOnFinger);
            } 
        }

        public FingerContainer parent { get; internal set; }
        public Transform transform { get; private set; }
        public FingerCollider fc   { get; private set; }       
        
        public PointOnHand(Transform transform, FingerCollider fc)
            :this(transform)
        {
            SetFC(fc);
        }

        public PointOnHand(Transform transform)
        {
            this.transform = transform;
        }

        public void SetFC(FingerCollider fc, MaestroHand mh = null)
        {
            this.fc = fc;
            this.fc.SetParentPOH(this);

            if (mh != null) {
                this.fc.SetParentHPI(mh);
            } else if (this.parent != null) {
                this.fc.SetParentHPI(this.parent.Parent.parent);
            }                
        }

        public bool Contacting { get { return fc.Contacting; } }
    }
}
