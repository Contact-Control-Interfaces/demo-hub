using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public class LatchingButtonBehavior : SingleButtonBehavior
    {
        protected bool Latched;

        [Tooltip("The percentage of the press depth the button should depress, when latched.")]
        [Range(0.0f, 1.0f)]
        public float LatchedDepressRatio;

        protected float LatchedPressRatio => 1f - LatchedDepressRatio;

        protected override void Awake()
        {
            base.Awake();

            onDown.AddListener(ToggleLatch);
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (buttonTrans != null) {
                Gizmos.color = Color.green;

                Vector3 direction = buttonTrans.TransformDirection(AxisToLocalDirection(pressDirection)).normalized;

                Gizmos.DrawLine(
                    buttonTrans.position - direction * PressDepth * LatchedPressRatio * buttonTrans.lossyScale.GetAxisValue(pressDirection), 
                    buttonTrans.position - direction * PressDepth * buttonTrans.lossyScale.GetAxisValue(pressDirection));
            }
        }

        protected override float StableAxisPosition {
            get {
                if (Latched) {
                    return base.StableAxisPosition - (PressDepth * LatchedPressRatio * (invertAxis ? -1f : 1f));
                }
                    else return base.StableAxisPosition;
            }
        }

        public void ToggleLatch()
        {
            Latched = !Latched;
        }
    }
}
