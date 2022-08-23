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
        private static float epsilon = 0.000001f;

        public SliderEvent onValueChanged;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;

            startLocalPosition = transform.localPosition;

            if (onValueChanged == null)
                onValueChanged = new SliderEvent();

            lastZ = float.MaxValue;

            CallEvent();
        }

        private void CallEvent()
        {
            float z = transform.localPosition.z;

            if (Mathf.Abs(z - lastZ) > epsilon) {

                z = Mathf.Clamp(z, -range, range);

                transform.localPosition = new Vector3(startLocalPosition.x, startLocalPosition.y, z);

                value = Mathf.InverseLerp(-range, range, z);
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
            mi.overrideAmplitudeFromScale(value);
        }

        public void setInteractableEffect(MaestroInteractable mi)
        {
            var ovr = mi.stayHaptics.Copy();
            ovr.Vibration = VibrationEffect.ConstructEffect((byte)(value * 128));
            mi.SetHapticOverride(ovr);
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
