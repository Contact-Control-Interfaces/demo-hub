using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Maestro
{
    public enum InteractionType
    {
        Static, /*OneHandPinch,*/ OneHandGrab, TwoHand
    };

    [System.Serializable]
    public class TouchEvent : UnityEvent<FingerCollider> { }

    public class MaestroInteractable : MonoBehaviour, IComparable<MaestroInteractable>
    {

        [Header("Configuration")]
        public InteractionType type = InteractionType.OneHandGrab;

        public bool isTool { get { return gripTransform != null; } }
        public bool IgnoreTaps;
        public bool UseRenderCenter;
        public bool maintainOrientation;
        public bool maintainPosition;
        public bool stayInHand;
        public bool SendHapticsToWholeHand;

        public Transform gripTransform;
        public Collider gripCollider;

        public bool isGrabbed { get; set; }

        [HideInInspector]
        public Rigidbody rb;

        [HideInInspector]
        public Renderer rend;

        [Header("Events")]
        public UnityEvent onGrab;
        public UnityEvent onRelease;
        public TouchEvent onTouch, unTouch;

        [Header("Haptics")]
        public HapticEffect haptics;

        [Header("Special Behavior")]
        public bool isPersistent = false;
        public float persistenceDuration = 0.0f;

        [HideInInspector]
        public byte? ResponseMotorAmplitude { private get; set; }
        public byte? ResponseVibrationEffect { private get; set; }

        private int oldLayer;

        public void Start()
        {
            rb = this.GetComponent<Rigidbody>();
            rend = this.GetComponent<Renderer>();
            isGrabbed = false;
            ResponseMotorAmplitude = ResponseVibrationEffect = null;
        }

        public void Touch(FingerCollider finger)
        {
            if (onTouch != null)
                onTouch.Invoke(finger);
            //if (finger.index == 1)
            //{ //index == 1 means index finger
            //    onPoked.Invoke();
            //}
        }

        public void Untouch(FingerCollider finger)
        {
            if (unTouch != null)
                unTouch.Invoke(finger);
        }

        public void Grab(int newLayer)
        {
            oldLayer = this.gameObject.layer;
            if (newLayer >= 0)
                this.gameObject.layer = newLayer;
            isGrabbed = true;
            onGrab.Invoke();
        }

        public void Release()
        {
            isGrabbed = false;
            this.gameObject.layer = oldLayer;
            ResponseMotorAmplitude = ResponseVibrationEffect = null;
            onRelease.Invoke();
        }

        public void OnCollisionEnter(Collision c)
        {
            //NOTHING
        }

        public void OnCollisionStay(Collision collision)
        {
            //NOTHING
        }

        public void OnCollisionExit(Collision collision)
        {
            //NOTHING
        }

        public Vector3 getFollowPoint()
        {
            return gripTransform == null ? (UseRenderCenter ? rend.bounds.center : this.transform.position) : gripTransform.position;
        }

        public byte? getMotorAmplitude()
        {
            return ResponseMotorAmplitude ?? haptics.Amplitude;
        }

        public byte? getVibrationEffect()
        {
            return ResponseVibrationEffect ?? haptics.Vibration;
        }

        public void setHaptics(byte? amp, byte? vib)
        {
            if (amp.HasValue)
                haptics.Amplitude = amp.Value;

            if (vib.HasValue)
                haptics.Vibration = vib.Value;
        }

        public void setAmplitudeFromScale(float scale)
        {
            haptics.Amplitude = (byte)(255 * scale);
        }

        public void setEffectFromScale(float scale)
        {
            haptics.Vibration = (byte)(128 * scale);
        }

        // Default comparer, TODO
        public int CompareTo(MaestroInteractable other)
        {
            if (other == null)
                return -1;

            //TODO add priority
            if (other.haptics.Amplitude != this.haptics.Amplitude) {
                return this.haptics.Amplitude.CompareTo(other.haptics.Amplitude);
            } else {
                return this.haptics.Vibration.CompareTo(other.haptics.Vibration);
            }
        }
    }

    class PrioritizeAmplitude : IComparer<MaestroInteractable>
    {
        public int Compare(MaestroInteractable x, MaestroInteractable y)
        {
            if (y != null && x != null) {
                if (y.haptics.Amplitude != x.haptics.Amplitude) {
                    return x.haptics.Amplitude.CompareTo(y.haptics.Amplitude);
                } else {
                    return x.haptics.Vibration.CompareTo(y.haptics.Vibration);
                }
            } else if (x != null) {
                return 1;
            } else {
                return -1;
            }
        }
    }

    class PrioritizeVibrationEffect : IComparer<MaestroInteractable>
    {
        public int Compare(MaestroInteractable x, MaestroInteractable y)
        {
            if (y != null && x != null) {
                if (y.haptics.Vibration != x.haptics.Vibration) {
                    return x.haptics.Vibration.CompareTo(y.haptics.Vibration);
                } else {
                    return x.haptics.Amplitude.CompareTo(y.haptics.Amplitude);
                }
            } else if (x != null) {
                return 1;
            } else {
                return -1;
            }
        }
    }
}
