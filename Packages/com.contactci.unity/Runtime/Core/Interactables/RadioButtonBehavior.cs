using System.Collections.Generic;
using Maestro.Vibration;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Maestro
{
    public class RadioButtonBehavior : ButtonBase
    {
        [Tooltip("LED object to indicate button state (optional)")]
        public ScriptableLight indicator;

        [Tooltip("Should this button stay down when pressed? Latched and unlatched buttons can be mixed")]
        public bool latched = true;  

        [Tooltip("Index of this button in the panel. Index is passed to event listeners.")]
        public int index;
        
        public event Action<int> OnLatch;

        protected override void ButtonPushed()
        {
	        base.ButtonPushed();
	        Latch_Internal();
        }

        protected override void ButtonReleased()
        {
	        base.ButtonReleased();
        }

        protected override void ApplyReturnForce()
        {
	        if (latched && isPressed)
		        return;
	        //else
	        base.ApplyReturnForce();
        }

        public void Latch()
        {
	        ButtonPushed();
        }

        private void Latch_Internal()
        {
			if (indicator != null)
				indicator.SetLight(true);
			if (latched)
			{
				buttonTrans.localPosition = ConstrainAxis(pressDirection, FullyPressedAxisPosition);
				rb.velocity=Vector3.zero;
			}

			OnLatch?.Invoke(index);
        }

		public void ResetLatch()
		{
			ButtonReleased();
			if (indicator != null)
				indicator.SetLight(false);
		}
    }
}