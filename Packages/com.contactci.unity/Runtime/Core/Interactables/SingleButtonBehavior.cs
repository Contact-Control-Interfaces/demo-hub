using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using Maestro.Vibration;

namespace Maestro
{
	[System.Serializable]
	public class ButtonEvent : UnityEvent<bool> { }

	public class SingleButtonBehavior : ButtonBase
	{
		public ButtonEvent onStateChanged;
		public UnityEvent onDown, onUp;

		protected override void ButtonPushed()
		{
			base.ButtonPushed();
			// Invoke events
			onDown?.Invoke();
			onStateChanged?.Invoke(true);
		}

		protected override void ButtonReleased()
		{
			base.ButtonReleased();
			
			// Invoke events
			onUp?.Invoke();
			onStateChanged?.Invoke(false);
		}
	}
}