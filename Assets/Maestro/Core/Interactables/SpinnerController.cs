using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public class SpinnerController : MonoBehaviour
    {
        public SliderBehavior slider;
        public ToggleSwitchBehavior toggleSwitch;

        public bool on;

        private Rigidbody spinner;
        private float lastValue;

        // Start is called before the first frame update
        void Start()
        {
            spinner = this.GetComponent<Rigidbody>();
            lastValue = 0f;
        }

        // Update is called once per frame
        void Update()
        {
            if (on) {
                float maxSpeed = lastValue * 10;
                float accel = lastValue * 10;

                //Debug.Log(maxSpeed);

                spinner.angularVelocity += accel * new Vector3(0, 1, 0);

                if (spinner.angularVelocity.magnitude > maxSpeed)
                    spinner.angularVelocity = new Vector3(0, 1, 0) * maxSpeed;
            }
        }

        public void UpdateFromSlider(float value)
        {
            //Debug.Log(value);

            lastValue = 1 - value;

            //Debug.Log(lastValue);
        }

        public void TurnOnOff(bool state)
        {
            this.on = state;
        }

        public void SetText(TextMesh text)
        {
            text.text = Mathf.RoundToInt(lastValue * 100) + "%";
        }
    }
}
