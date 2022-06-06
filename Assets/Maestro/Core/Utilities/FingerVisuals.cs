using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Maestro.PhysicsGrabManager;

namespace Maestro
{
    public class FingerVisuals : MonoBehaviour
    {
        public Transform thumbVisual;
        public Transform indexVisual;
        public Transform middleVisual;
        public Transform ringVisual;
        public Transform littleVisual;

        public TouchingFingers yubi;

        void Start()
        {
            UpdateAll();
        }

        public void SetMask(TouchingFingers touching)
        {
            yubi = touching;
            UpdateAll();
        }

        private void UpdateAll()
        {
            thumbVisual.gameObject.SetActive(yubi.HasFlag(TouchingFingers.ThumbTip));
            indexVisual.gameObject.SetActive(yubi.HasFlag(TouchingFingers.IndexTip));
            middleVisual.gameObject.SetActive(yubi.HasFlag(TouchingFingers.MiddleTip));
            ringVisual.gameObject.SetActive(yubi.HasFlag(TouchingFingers.RingTip));
            littleVisual.gameObject.SetActive(yubi.HasFlag(TouchingFingers.LittleTip));
        }
    }
}
