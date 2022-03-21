using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Maestro
{
	[System.Serializable]
	public class ButtonEvent : UnityEvent<bool> { }

	public class WobblyButtonOutput : MonoBehaviour
	{

		public Vector3 origin;
		private Rigidbody rb;

		public Transform buttonTrans;

		public bool buttonDown;
		public bool On;
		public float buttonSensitivity;

		public float slideForce;

		public static AudioClip down, up;

		public float pitchOffset = 0.0f;

		private AudioSource source;

		public ButtonEvent onStateChanged;
		public UnityEvent onDown, onUp;

		public List<Collider> toIgnore;

		void Awake()
		{
			if (!down) down = Resources.Load<AudioClip>("Sounds/button_down");
			if (!up) up = Resources.Load<AudioClip>("Sounds/button_up");

			source = this.gameObject.AddComponent<AudioSource>();
			source.playOnAwake = false;
			source.volume = 0.2f;

			origin = buttonTrans.localPosition;
			rb = buttonTrans.GetComponent<Rigidbody>();
			rb.velocity = Vector3.zero;

			Collider temp = rb.GetComponent<Collider>();

			if (toIgnore != null && toIgnore.Count > 0) {
				foreach (Collider c in toIgnore) {
					Physics.IgnoreCollision(temp, c, true);
                }
            }
		}

		void Update()
		{
			float y = buttonTrans.localPosition.y;

			if (y < origin.y) {
				rb.velocity = buttonTrans.up * slideForce;

				if (y < origin.y - buttonSensitivity) {
					if (!buttonDown) {
						ButtonPushed();
					}
				}
			} else {
				if (buttonDown) {
					ButtonReleased();
				}
				rb.velocity = Vector3.zero;
			}

			buttonTrans.localPosition = new Vector3(origin.x, Mathf.Clamp(y, origin.y - buttonSensitivity, origin.y), origin.z);
		}

		void ButtonPushed()
		{
			Debug.Log("Button pushed: " + this.name);

			buttonDown = true;
			On = !On;

			// Invoke events
			onDown.Invoke();
			if (onStateChanged != null)
				onStateChanged.Invoke(true);

			// Play press sound
			source.clip = down;
			source.pitch = pitchOffset + Random.Range(0.95f, 1.05f);
			source.Play();
		}

		void ButtonReleased()
		{
			Debug.Log("Button released: " + this.name);

			buttonDown = false;
			buttonTrans.localPosition = Vector3.zero;
			rb.velocity = Vector3.zero;

			// Invoke events
			onUp.Invoke();
			if (onStateChanged != null)
				onStateChanged.Invoke(false);

			// Play release sound
			source.clip = up;
			source.pitch = pitchOffset + Random.Range(0.95f, 1.05f);
			source.Play();
		}
	}
}