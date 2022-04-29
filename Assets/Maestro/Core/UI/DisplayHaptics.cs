using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Maestro.UI
{
    public class DisplayHaptics : MonoBehaviour
    {
        private static string toggleButton = "ToggleHapticsDisplay";

        private static Color baseGray = new Color((float)0x92 / 0xff, (float)0x94 / 0xff, (float)0x97 / 0xff);
        private static Color color = new Color((float)0x87 / 0xff, (float)0xd1 / 0xff, (float)0xd0 / 0xff);

        public WhichHand whichHand;
        public IMaestroHand hand;
        public RectTransform baseImage;

        public Transform ThumbVibration;
        public Transform IndexVibration;
        public Transform MiddleVibration;
        public Transform RingVibration;
        public Transform LittleVibration;

        public Image ThumbForceFeedback;
        public Image IndexForceFeedback;
        public Image MiddleForceFeedback;
        public Image RingForceFeedback;
        public Image LittleForceFeedback;

        private bool inputDefined = false;
        private bool toggled = true;
        private bool lastButtonDown = false;

        void Start()
        {
            if (baseImage == null && this.transform.childCount > 0) {
                baseImage = (RectTransform)this.transform.GetChild(0);
            }

            // Auto get sub elements if they aren't set already
            if (ThumbVibration == null) {
                int i = 0;
                Transform lines = baseImage.GetChild(0);
                ThumbForceFeedback = lines.GetChild(i++).GetComponent<Image>();
                IndexForceFeedback = lines.GetChild(i++).GetComponent<Image>();
                MiddleForceFeedback = lines.GetChild(i++).GetComponent<Image>();
                RingForceFeedback = lines.GetChild(i++).GetComponent<Image>();
                LittleForceFeedback = lines.GetChild(i++).GetComponent<Image>();

                Transform dots = baseImage.GetChild(1);
                i = 0;
                ThumbVibration = dots.GetChild(i++);
                IndexVibration = dots.GetChild(i++);
                MiddleVibration = dots.GetChild(i++);
                RingVibration = dots.GetChild(i++);
                LittleVibration = dots.GetChild(i++);
            }

            if (hand == null || hand.whichHand != whichHand) {
                // Find first matching hand
                IMaestroHand[] hands = GameObject.FindObjectsOfType<IMaestroHand>();
                if (hands.Length > 0)
                    hand = hands.FirstOrDefault(x => x.whichHand == this.whichHand);
            }

            // Issue warning if the toggle button isn't defined
            try {
                Input.GetButton(toggleButton);
                inputDefined = true;
            } catch (Exception) {
                Debug.LogWarning($"Input '{toggleButton}' is not bound! Define it for a shortcut to disable the haptics UI.");
                inputDefined = false;
            }
        }

        void Update()
        {
            if (hand != null) MatchHaptics(hand.lastHaptics);

            if (inputDefined) {
                bool buttonDown = Input.GetButtonDown(toggleButton);

                if (buttonDown && !lastButtonDown) {
                    toggled = !toggled;
                    baseImage.gameObject.SetActive(toggled);
                }

                lastButtonDown = buttonDown;
            }
        }

        /*
         * Updates the overlay to match haptics
         */
        private void MatchHaptics(MaestroHapticContext haptics)
        {
            ThumbVibration.gameObject.SetActive(isVibrating(haptics.ThumbVibrationEffect));
            IndexVibration.gameObject.SetActive(isVibrating(haptics.IndexVibrationEffect));
            MiddleVibration.gameObject.SetActive(isVibrating(haptics.MiddleVibrationEffect));
            RingVibration.gameObject.SetActive(isVibrating(haptics.RingVibrationEffect));
            LittleVibration.gameObject.SetActive(isVibrating(haptics.LittleVibrationEffect));

            // Don't enable/disable, just use opacity
            ThumbForceFeedback.color = Color.Lerp(baseGray, color, (float)haptics.ThumbAmplitude.GetValueOrDefault() / 0xff);
            IndexForceFeedback.color = Color.Lerp(baseGray, color, (float)haptics.IndexAmplitude.GetValueOrDefault() / 0xff);
            MiddleForceFeedback.color = Color.Lerp(baseGray, color, (float)haptics.MiddleAmplitude.GetValueOrDefault() / 0xff);
            RingForceFeedback.color = Color.Lerp(baseGray, color, (float)haptics.RingAmplitude.GetValueOrDefault() / 0xff);
            LittleForceFeedback.color = Color.Lerp(baseGray, color, (float)haptics.LittleAmplitude.GetValueOrDefault() / 0xff);
        }

        private bool isVibrating(Vibration.VibrationEffect effect)
        {
            return effect != null && effect != Vibration.VibrationEffect.None;
        }
    }
}
