using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialBoolEvent : UnityEvent<bool> { }

public class NestedDialBehavior : MonoBehaviour
{
    public Rigidbody TopDial, BottomDial;

    public bool DialPushed;
    private bool lastDialPushed;

    public DialBoolEvent onDialButtonChanged;

    public UnityEvent onDialButtonPushed, onDialButtonReleashed;

    public int TopDialTicks = 0;
    public int TopDialTick;

    public int BottomDialTicks = 0;
    public int BottomDialTick;

    private int lastTopTick;
    private int lastBottomTick;
    //public float angle = 90f;

    private ConfigurableJoint top, bottom;

    private int topRegister, bottomRegister;

    private Vector3 start;
    private float offset;
    private float threshold = 0.0001f;

    private float lastTopAngle, lastBottomAngle;

    private AudioSource buttonSource, topDialSource, bottomDialSource;
    private AudioClip btnDown, btnUp;

    private MaestroInteractable mi, mi2;
    public float tickHapticDelay = 0.01f;
    public byte topTickHapticEffect = 1;
    public byte bottomTickHapticEffect = 5;
    private byte originalTopEffect, originalBottomEffect;
    private float timeElapsedSinceTopTick, timeElapsedSinceBottomTick;

    List<FingerTipCollider> topFTCs, bottomFTCs;

    void Start()
    {
        // Init Audio Sources
        buttonSource = TopDial.gameObject.AddComponent<AudioSource>();
        buttonSource.playOnAwake = false;
        buttonSource.volume = 0.2f;

        topDialSource = TopDial.gameObject.AddComponent<AudioSource>();
        topDialSource.playOnAwake = false;
        topDialSource.volume = 0.2f;
        topDialSource.pitch = 2f;

        bottomDialSource = BottomDial.gameObject.AddComponent<AudioSource>();
        bottomDialSource.playOnAwake = false;
        bottomDialSource.volume = 0.2f;
        bottomDialSource.pitch = 1.0f;

        // Load Audio Clips
        topDialSource.clip = bottomDialSource.clip = Resources.Load<AudioClip>("Sounds/tumbler");
        buttonSource.clip = btnDown = Resources.Load<AudioClip>("Sounds/button_down");
        btnUp = Resources.Load<AudioClip>("Sounds/button_up");

        // Init physics components
        Physics.IgnoreCollision(TopDial.gameObject.GetComponent<Collider>(), BottomDial.gameObject.GetComponent<Collider>(), true);

        top = TopDial.gameObject.GetComponent<ConfigurableJoint>();
        bottom = BottomDial.gameObject.GetComponent<ConfigurableJoint>();

        // Init events
        topRegister = bottomRegister = 0;

        if (onDialButtonPushed == null)
            onDialButtonPushed = new UnityEvent();

        if (onDialButtonReleashed == null)
            onDialButtonReleashed = new UnityEvent();

        // Init dial behavior
        start = TopDial.transform.localPosition;
        offset = top.linearLimit.limit;

        // Init haptics stuff
        mi = TopDial.gameObject.GetComponent<MaestroInteractable>();
        mi2 = BottomDial.gameObject.GetComponent<MaestroInteractable>();
        originalTopEffect = mi.VibrationEffect;
        originalBottomEffect = mi2.VibrationEffect;
        topFTCs = new List<FingerTipCollider>();
        bottomFTCs = new List<FingerTipCollider>();
    }

    private float getNearestTick(float angle, int ticks, out int tick)
    {
        float delta = 360f / ticks;

        float currentTick = angle / delta;
        //Debug.Log(angle + " " + currentTick);

        tick = Mathf.RoundToInt(currentTick) % ticks;

        return tick * delta;
    }

    private void Update()
    {
        if (TopDialTicks > 0)
        {
            float angle = TopDial.transform.localRotation.eulerAngles.y;

            top.targetRotation = Quaternion.Euler(-getNearestTick(angle, TopDialTicks, out TopDialTick), 0, 0);

            if (lastTopTick != TopDialTick)
            {
                topDialSource.Play();

                // Trigger haptics
                timeElapsedSinceTopTick = 0f;
                mi.VibrationEffect = topTickHapticEffect;

                /*foreach(FingerTipCollider ftc in topFTCs)
                {
                    ftc.vocb.VibrationEffect = topTickHapticEffect;
                    ftc.vocb.Prop();
                }*/

            } else {
                timeElapsedSinceTopTick += Time.deltaTime;
                if (timeElapsedSinceTopTick > tickHapticDelay)
                {
                    mi.VibrationEffect = originalTopEffect;
                    timeElapsedSinceTopTick = -1f;
                }
            }

            lastTopAngle = angle;
            lastTopTick = TopDialTick;
        }

        if (BottomDialTicks > 0)
        {
            float angle = BottomDial.transform.localRotation.eulerAngles.y;

            getNearestTick(angle, BottomDialTicks, out BottomDialTick); //Don't set target rotation unless we want it to snap to positions

            if (lastBottomTick != BottomDialTick)
            {
                bottomDialSource.Play();

                // Trigger haptics
                timeElapsedSinceBottomTick = 0f;
                mi2.VibrationEffect = bottomTickHapticEffect;

                /*foreach (FingerTipCollider ftc in bottomFTCs)
                {
                    ftc.vocb.VibrationEffect = bottomTickHapticEffect;
                    ftc.vocb.Prop();
                }*/
            } else {
                timeElapsedSinceBottomTick += Time.deltaTime;
                if (timeElapsedSinceBottomTick > tickHapticDelay) {
                    mi2.VibrationEffect = originalBottomEffect;
                    timeElapsedSinceBottomTick = -1f;
                }
            }

            lastBottomAngle = angle;
            lastBottomTick = BottomDialTick;
        }

        DialPushed = (start.y - TopDial.transform.localPosition.y - offset) > threshold;

        if (DialPushed && !lastDialPushed)
        {
            buttonSource.clip = btnDown;
            buttonSource.pitch = Random.Range(2.9f, 3.1f);
            buttonSource.Play();

            if (onDialButtonChanged != null)
                onDialButtonChanged.Invoke(true);

            onDialButtonPushed.Invoke();
        } else if (!DialPushed && lastDialPushed)
        {
            buttonSource.clip = btnUp;
            buttonSource.pitch = Random.Range(2.9f, 3.1f);
            buttonSource.Play();

            if (onDialButtonChanged != null)
                onDialButtonChanged.Invoke(false);

            onDialButtonReleashed.Invoke();
        }

        lastDialPushed = DialPushed;
    }

    public void RegisterTop(FingerTipCollider ftc)
    {
        topFTCs.Add(ftc);

        topRegister++;

        if (top)
            top.angularXMotion = ConfigurableJointMotion.Free;
    }

    public void UnregisterTop(FingerTipCollider ftc)
    {
        topRegister--;
        topFTCs.Remove(ftc);
    }

    public void RegisterBottom(FingerTipCollider ftc)
    {
        bottomFTCs.Add(ftc);

        bottomRegister++;
        if (bottom)
            bottom.angularXMotion = ConfigurableJointMotion.Free;
    }

    public void UnregisterBottom(FingerTipCollider ftc)
    {
        bottomFTCs.Remove(ftc);

        bottomRegister--;

        Quaternion temp = BottomDial.rotation;

        if (bottomRegister <= 0)
        {
            //bottom.targetRotation = temp;
            //bottom.angularXMotion = ConfigurableJointMotion.Locked;
        }
    }
}
