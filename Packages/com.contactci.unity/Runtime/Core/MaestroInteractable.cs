using Maestro.Vibration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

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
        public TouchEvent onTouch, unTouch, whileTouch;

        [HideInInspector] public HapticEffect hapticOverride { get; private set; }


        [Header("Haptics - Touch Begin")]
        public HapticEffect startHaptics = new HapticEffect { Amplitude = 200, Vibration = new None() };
        [Header("Haptics - Touch Sustain")]
        [FormerlySerializedAs("haptics")]
        public HapticEffect stayHaptics = new HapticEffect { Amplitude = 200, Vibration = new None() };
        [Header("Haptics - Touch End")]
        public HapticEffect exitHaptics = new HapticEffect { Amplitude = 200, Vibration = new None() };
        
        [Header("Special Behavior")]
        public bool isPersistent = false;
        public float persistenceDuration = 0.0f;

        public HapticEffect currentHaptics
        {
            get
            {
                return hapticOverride ?? _currentHaptics;
                //TODO! We need to reset OneShot effects once played
                //maybe ask device when it's done?
                if (hapticOverride != null)
                {
                    var ret = hapticOverride;
                    if(hapticOverride.Vibration.OneShot)
                        ResetOverride();
                    return ret;
                }
                return _currentHaptics; 
                
            }
            private set => _currentHaptics = value;
        }

        private int oldLayer;
        private HapticEffect _currentHaptics;

        public void Start()
        {
            rb = this.GetComponent<Rigidbody>();
            rend = this.GetComponent<Renderer>();
            isGrabbed = false;
        }

        public void Touch(FingerCollider finger)
        {
            if (onTouch != null)
                onTouch.Invoke(finger);
            //if (finger.index == 1)
            //{ //index == 1 means index finger
            //    onPoked.Invoke();
            //}
            currentHaptics = startHaptics;
        }

        public void WhileTouch(FingerCollider finger)
        {
            if (whileTouch != null)
                whileTouch.Invoke(finger);
            currentHaptics = stayHaptics;
        }

        public void Untouch(FingerCollider finger)
        {
            if (unTouch != null)
                unTouch.Invoke(finger);
            currentHaptics = exitHaptics;
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
            currentHaptics = null;
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

        public void overrideAmplitudeFromScale(float scale)
        {
            hapticOverride.Amplitude = (byte)(255 * scale);
        }

        public void SetHapticOverride(HapticEffect haptics)
        {
            hapticOverride = haptics;
        }

        public void ResetOverride()
        {
            hapticOverride = null;
        }

        
        // Default comparer, TODO
        public int CompareTo(MaestroInteractable other)
        {
            if (other == null)
                return -1;

            //TODO add priority
            return this.currentHaptics.CompareAmplitudesFirst(other.currentHaptics);
        }
    }

    class PrioritizeAmplitude : IComparer<MaestroInteractable>
    {
        public int Compare(MaestroInteractable x, MaestroInteractable y)
        {
            return x.currentHaptics.CompareAmplitudesFirst(y.currentHaptics);
        }
    }

    class PrioritizeVibrationEffect : IComparer<MaestroInteractable>
    {
        public int Compare(MaestroInteractable x, MaestroInteractable y)
        {
            return x.currentHaptics.CompareVibrationEffectsFirst(y.currentHaptics);
        }
    }
}
