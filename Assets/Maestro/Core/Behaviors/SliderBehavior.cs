using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class SliderEvent : UnityEvent<float> { }

public class SliderBehavior : MonoBehaviour {

    private Rigidbody rb;
    private Vector3 startLocalPosition;

    public float range;
    public float value = 0.5f;
    public int textScale = 255;
    public string suffix = "";
    public bool showSpecialValues = true;

    private float lastZ;

    public SliderEvent onValueChanged;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        startLocalPosition = transform.localPosition;

        if (onValueChanged == null)
            onValueChanged = new SliderEvent();
        

        lastZ = 0;

        CallEvent();
    }

    private void CallEvent()
    {
        float z = transform.localPosition.z;

        if (z != lastZ)
        {
            if (z > range)
                z = range;
            else if (z < -range)
                z = -range;

            transform.localPosition = new Vector3(startLocalPosition.x, startLocalPosition.y, z);

            value = Mathf.InverseLerp(-range, range, transform.localPosition.z);

            onValueChanged.Invoke(value);
        }

        lastZ = z;
    }
	
	// Update is called once per frame
	void Update () {
        CallEvent();
    }

    public void setInteractableAmplitude(MaestroInteractable mi)
    {
        mi.setAmplitudeFromScale(value);
    }

    public void setInteractableEffect(MaestroInteractable mi)
    {
        mi.setEffectFromScale(value);
    }

    public void setTextMesh(TextMesh tm)
    {
        byte result = (byte)(textScale * value);
        if (showSpecialValues && result == 0)
            tm.text = "OFF";
        else if (showSpecialValues && result >= textScale)
            tm.text = "MAX";
        else
            tm.text = result + suffix;
    }
}
