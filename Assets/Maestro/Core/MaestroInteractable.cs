using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum InteractionType
{
    Static, /*OneHandPinch,*/ OneHandGrab, TwoHand
};

[System.Serializable]
public class TouchEvent : UnityEvent<FingerTipCollider> { }

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
    public byte Amplitude = 200;
    public byte VibrationEffect = 1;

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

    public void Touch(FingerTipCollider finger)
    {
        if (onTouch != null)
            onTouch.Invoke(finger);
        //if (finger.index == 1)
        //{ //index == 1 means index finger
        //    onPoked.Invoke();
        //}
    }

    public void Untouch(FingerTipCollider finger)
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
        return ResponseMotorAmplitude ?? Amplitude;
    }

    public byte? getVibrationEffect()
    {
        return ResponseVibrationEffect ?? VibrationEffect;
    }

    public void setHaptics(byte? amp, byte? vib)
    {
        if (amp.HasValue)
            Amplitude = amp.Value;

        if (vib.HasValue)
            VibrationEffect = vib.Value;
    }

    public void setAmplitudeFromScale(float scale)
    {
        Amplitude = (byte)(255 * scale);
    }

    public void setEffectFromScale(float scale)
    {
        VibrationEffect = (byte)(128 * scale);
    }

    public void SetDribbleOverride(FingerTipCollider ftc)
    {
        if (ftc.pocb && ftc.vocb) {
            ftc.pocb.DribbleOverride = true;
            ftc.pocb.DribbleWait = 0f;
            ftc.pocb.Amplitude = this.Amplitude;
            ftc.vocb.VibrationEffect = this.VibrationEffect;
            //Debug.Log("DRIB");
        }
    }

    public void UnsetDribbleOverride(FingerTipCollider ftc)
    {
        /*if (ftc.pocb)
        {
            ftc.pocb.DribbleOverride = false;
            Debug.Log("DROB");
        }*/
    }

    // Default comparer, TODO
    public int CompareTo(MaestroInteractable other)
    {
        if (other == null)
            return -1;

        //TODO add priority
        if (other.Amplitude != this.Amplitude) {
            return this.Amplitude.CompareTo(other.Amplitude);
        } else {
            return this.VibrationEffect.CompareTo(other.VibrationEffect);
        }
    }
}

class PrioritizeAmplitude : IComparer<MaestroInteractable>
{
    public int Compare(MaestroInteractable x, MaestroInteractable y)
    {
        if (y != null && x != null) {
            if (y.Amplitude != x.Amplitude) {
                return x.Amplitude.CompareTo(y.Amplitude);
            } else {
                return x.VibrationEffect.CompareTo(y.VibrationEffect);
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
            if (y.VibrationEffect != x.VibrationEffect) {
                return x.VibrationEffect.CompareTo(y.VibrationEffect);
            } else {
                return x.Amplitude.CompareTo(y.Amplitude);
            }
        } else if (x != null) {
            return 1;
        } else {
            return -1;
        }
    }
}
