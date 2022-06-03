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
    public enum WhichFinger
    {
        Thumb, Index, Middle, Ring, Little, Palm
    };

    public enum PointOnFinger
    {
        Tip, Middle, Base
    };

    public struct MaestroIndex
    {
        public WhichFinger finger { get; set; }
        public PointOnFinger point { get; set; }

        public MaestroIndex(WhichFinger finger, PointOnFinger point)
        {
            this.finger = finger;
            this.point = point;
        }
        
        public override string ToString()
        {
            return string.Format("({0}, {1})", ToString(finger), ToString(point));
        }

        public static string ToString(WhichFinger whichFinger)
        {
            string result = "";
            switch (whichFinger) {
                case WhichFinger.Thumb: result = "Thumb"; break;
                case WhichFinger.Index: result = "Index"; break;
                case WhichFinger.Middle: result = "Middle"; break;
                case WhichFinger.Ring: result = "Ring"; break;
                case WhichFinger.Little: result = "Little"; break;
                case WhichFinger.Palm: result = "Palm"; break;
            }
            return result;
        }

        public static string ToString(PointOnFinger pof)
        {
            string result = "";
            switch (pof) {
                case PointOnFinger.Base: result = "Base"; break;
                case PointOnFinger.Middle: result = "Middle"; break;
                case PointOnFinger.Tip: result = "Tip"; break;
            }
            return result;
        }
    }

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
        public PointOnHand Middle { get; private set; }
        public PointOnHand Base { get; private set; }

        public CapsuleCollider Distal { get; private set; }
        public CapsuleCollider Proximal { get; private set; }
        public CapsuleCollider Metacarpal { get; private set; }

        public FingerContainer(PointOnHand tip, PointOnHand middle, PointOnHand fingerBase, CapsuleCollider distal, CapsuleCollider proximal, CapsuleCollider metacarpal)
        {
            this.Tip = tip;
            this.Tip.whereOnFinger = PointOnFinger.Tip;
            this.Tip.parent = this;
            this.Middle = middle;
            this.Middle.whereOnFinger = PointOnFinger.Middle;
            this.Middle.parent = this;
            this.Base = fingerBase;
            this.Base.whereOnFinger = PointOnFinger.Base;
            this.Base.parent = this;
            this.Distal = distal;
            this.Proximal = proximal;
            this.Metacarpal = metacarpal;
        }

        public PointOnHand this[PointOnFinger pof] {
            get {
                switch (pof) {
                    case PointOnFinger.Tip: return Tip;
                    case PointOnFinger.Middle: return Middle;
                    case PointOnFinger.Base: return Base;
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
            yield return Base;
            yield return Middle;
            yield return Tip;
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
