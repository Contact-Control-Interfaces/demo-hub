using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using Maestro.Vibration;

namespace Maestro
{

	public enum ButtonAxis
	{
		LocalX, LocalY, LocalZ,
		//WorldX, WorldY, WorldZ
	};

	public static class ButtonAxisExtensions
    {
		public static float GetAxisValue(this Vector3 vector, ButtonAxis axis)
		{
			switch (axis) {
				default: throw new NotImplementedException($"No corresponding value defined for axis {axis.ToString()}");
				case ButtonAxis.LocalX: return vector.x;
				case ButtonAxis.LocalY: return vector.y;
				case ButtonAxis.LocalZ: return vector.z;
			}
		}

		public static void SetAxisValue(this Vector3 vector, ButtonAxis axis, float value)
		{
			switch (axis) {
				default: throw new NotImplementedException($"No corresponding value defined for axis {axis.ToString()}");
				case ButtonAxis.LocalX: vector.x = value; break;
				case ButtonAxis.LocalY: vector.y = value; break;
				case ButtonAxis.LocalZ: vector.z = value; break;
			}
		}
	}

	public abstract class ButtonBase : MonoBehaviour
	{
		public Vector3 origin;
		protected Rigidbody rb;

		public Transform buttonTrans;
		public ButtonAxis pressDirection;
		public bool invertAxis;

		public bool isPressed;
		public float PressDepth;

		public float slideForce;

		public static AudioClip down, up;

		public float pitchOffset = 0.0f;

		private AudioSource source;
		[HideInInspector]
		public Collider[] ignored;
		
		public MaestroInteractable Interactable;

		[Header("Haptics - Button Pressed"), Tooltip("Effect played when button hits bottom of travel")]
		public HapticEffect pressedHaptics = new HapticEffect { Amplitude = 255, Vibration = new StrongClick(FourOptions._100){OneShot = true} };

		[Header("Collision ignore"), Tooltip("Which direction to search for colliding entities")]
		public Axis castDirection = Axis.NegY;
		[Tooltip("How far to look for collisions")]
		public float castDistance = 0.1f;
		void Awake()
		{
			if (!down) down = Resources.Load<AudioClip>("Sounds/button_down");
			if (!up) up = Resources.Load<AudioClip>("Sounds/button_up");

			source = this.gameObject.AddComponent<AudioSource>();
			source.playOnAwake = false;
			source.volume = 0.2f;

			if (buttonTrans == null)
				buttonTrans = GetComponentInChildren<Rigidbody>().transform;

			if (Interactable == null)
				Interactable = GetComponentInChildren<MaestroInteractable>();

			rb = buttonTrans.GetComponent<Rigidbody>();
			rb.velocity = Vector3.zero;
			origin = buttonTrans.localPosition;

			Collider temp = rb.GetComponent<Collider>();

			//Search for colliders interfering with the button and ignore interactions
			RaycastColliders();
		}

		void Update()
		{
			if (BeingPressed()) {
				if (!isPressed && FullyPressed()) {
						ButtonPushed();
				}
			} else {
				if (isPressed) {
					ButtonReleased();
				}
			}

			ApplyReturnForce();

			float clampedPosition = FullyPressedAxisPosition < StableAxisPosition ?
				Mathf.Clamp(AxisPosition, FullyPressedAxisPosition, StableAxisPosition) :
				Mathf.Clamp(AxisPosition, StableAxisPosition, FullyPressedAxisPosition);

			buttonTrans.localPosition = ConstrainAxis(pressDirection, clampedPosition);
		}

		private void RaycastColliders()
		{
			Vector3 direction = axisToDirection(castDirection);

			RaycastHit[] hits = Physics.RaycastAll(
				new Ray(this.transform.position - (direction * castDistance / 2), direction), 
				castDistance);

			if (hits.Length > 0) {
				List<Collider> _ignored = new List<Collider>();

				Collider[] allColliders = this.GetComponentsInChildren<Collider>();

				Debug.Log($"Found {hits.Length} colliders to ignore for object {this.gameObject.name}");

				foreach (RaycastHit hit in hits) {
					if (Array.IndexOf(allColliders, hit.collider) < 0) {
						_ignored.Add(hit.collider);

						foreach (Collider c in allColliders) {
							Physics.IgnoreCollision(hit.collider, c);
						}
					}
				}

				ignored = _ignored.ToArray();
			}
		}
	
		protected virtual void ApplyReturnForce()
		{
			// Apply a force opposing the press
			rb.velocity = buttonTrans.TransformDirection(AxisToLocalDirection(pressDirection)).normalized * slideForce;
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;

			Vector3 direction = buttonTrans.TransformDirection(AxisToLocalDirection(pressDirection)).normalized;

			//Gizmos.DrawRay(new Ray(buttonTrans.position, direction));

			//Gizmos.color = Color.yellow;

			Gizmos.DrawLine(buttonTrans.position, buttonTrans.position + direction * PressDepth);
		}

		protected bool BeingPressed() {
			return invertAxis ? AxisPosition > StableAxisPosition : AxisPosition < StableAxisPosition;
        }

		protected float StableAxisPosition => origin.GetAxisValue(pressDirection);
		
		protected bool FullyPressed() {
				return invertAxis ? AxisPosition >= FullyPressedAxisPosition : AxisPosition <= FullyPressedAxisPosition;
			}

		protected float AxisPosition => buttonTrans.localPosition.GetAxisValue(pressDirection);

		protected float FullyPressedAxisPosition => origin.GetAxisValue(pressDirection) - (PressDepth * (invertAxis ? -1f : 1f));

		private Vector3 AxisToLocalDirection(ButtonAxis axis) {
			Vector3 result;

			switch (axis) {
				default: throw new NotImplementedException($"No direction defined for axis {axis.ToString()}");
				case ButtonAxis.LocalX: result = Vector3.right; break;
				case ButtonAxis.LocalY: result = Vector3.up; break;
				case ButtonAxis.LocalZ: result = Vector3.forward; break;
			}

			return invertAxis ? -result : result;
        }
		protected Vector3 ConstrainAxis(ButtonAxis axis, float position)
		{
			return axis switch
			{
				ButtonAxis.LocalX => new Vector3(position, origin.y, origin.z),
				ButtonAxis.LocalY => new Vector3(origin.x, position, origin.z),
				ButtonAxis.LocalZ => new Vector3(origin.x, origin.y, position),
				_ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
			};
		}
		
		private Vector3 axisToDirection(Axis axis)
		{
			switch (axis) {
				default:
				case Axis.X: return this.transform.right;
				case Axis.Y: return this.transform.up;
				case Axis.Z: return this.transform.forward;
				case Axis.NegX: return -axisToDirection(Axis.X);
				case Axis.NegY: return -axisToDirection(Axis.Y);
				case Axis.NegZ: return -axisToDirection(Axis.Z);
			}
		}
		
		
        protected virtual void ButtonPushed()
		{
			Debug.Log("Button pushed: " + this.name);

			isPressed = true;

			// Play press sound
			source.clip = down;
			source.pitch = pitchOffset + UnityEngine.Random.Range(0.95f, 1.05f);
			source.Play();
			
			// Play haptics
			Interactable.SetHapticOverride(pressedHaptics);
		}

		protected virtual void ButtonReleased()
		{
			Debug.Log("Button released: " + this.name);

			isPressed = false;
			buttonTrans.localPosition = origin;
			rb.velocity = Vector3.zero;

			// Play release sound
			source.clip = up;
			source.pitch = pitchOffset + UnityEngine.Random.Range(0.95f, 1.05f);
			source.Play();
			
			// clear haptics if set
			Interactable.ResetOverride();
		}
	}
}