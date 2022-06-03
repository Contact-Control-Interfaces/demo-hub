using Maestro.Vibration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Maestro
{

    [Serializable]
    public class SliderEvent : UnityEvent<float> { }

    public class SliderBehavior : MonoBehaviour
    {

        private Rigidbody rb;
        private Vector3 startLocalPosition;

        public TextMesh outputTextMesh;

        public float range;
        public float value = 0.5f;

        public float outputScale = 1f;
        public float outputOffset = 0f;
        public float outputValue = 0f;

        public string suffix = "";
        public bool showSpecialValues = true;

        private float lastZ;

        public SliderEvent onValueChanged;

        // Use this for initialization
        void Start()
        {
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

            if (z != lastZ) {
                if (z > range)
                    z = range;
                else if (z < -range)
                    z = -range;

                transform.localPosition = new Vector3(startLocalPosition.x, startLocalPosition.y, z);

                value = Mathf.InverseLerp(-range, range, transform.localPosition.z);
                outputValue = outputOffset + outputScale * value;

                if (outputTextMesh != null) {
                    outputTextMesh.text = OutputText;
                }

                onValueChanged.Invoke(outputValue);
            }

            lastZ = z;
        }

        // Update is called once per frame
        void Update()
        {
            CallEvent();
        }

        public void setInteractableAmplitude(MaestroInteractable mi)
        {
            mi.setAmplitudeFromScale(value);
        }

        public void setInteractableEffect(MaestroInteractable mi)
        {
            mi.haptics.Vibration = VibrationEffect.ConstructEffect((byte)(value * 128));
        }

        public string OutputText {
            get {
                if (showSpecialValues && outputValue == 0) {
                    return "OFF";
                } else if (showSpecialValues && outputValue >= outputOffset + outputScale) {
                    return "MAX";
                } else {
                    return outputValue.ToString("0.00") + suffix;
                }
            }
        }
    }
}
