using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Maestro
{
    public class RockerBehavior : MonoBehaviour
    {
        public bool Toggled;
        private bool lastToggled = false;

        public ConfigurableJoint cj;

        public ToggleEvent onToggleChanged;
        public UnityEvent onToggleOn, onToggleOff;

        public TextMesh debug;

        private AudioSource source;
        private float upPitch = 2.0f;
        private float downPitch = 1.0f;

        private float high, low;
        private Quaternion spawnRotation;


        public List<Collider> toIgnore;
        public float switchThreshold = 1f;

        private bool resting = true;

        private int touchingBRT, touchingDIM;

        public Collider brt, dim;

        // Start is called before the first frame update
        void Start()
        {
            spawnRotation = this.transform.localRotation;

            touchingBRT = touchingDIM = 0;

            Toggled = false;
            UpdateToggled();

            if (onToggleOn == null)
                onToggleOn = new UnityEvent();

            if (onToggleOff == null)
                onToggleOff = new UnityEvent();

            source = GetComponent<AudioSource>();
            if (source == null)
                source = this.gameObject.AddComponent<AudioSource>();

            source.volume = 0.5f;
            source.playOnAwake = false;
            source.clip = Resources.Load<AudioClip>("Sounds/switch");

            high = cj.highAngularXLimit.limit;
            low = cj.lowAngularXLimit.limit;

            if (toIgnore != null && toIgnore.Count > 1) {
                for (int i = 0; i < toIgnore.Count - 1; i++) {
                    for (int j = i + 1; j < toIgnore.Count; j++) {
                        Physics.IgnoreCollision(toIgnore[i], toIgnore[j], true);
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            UpdateToggled();
        }

        public void TouchBRT()
        {
            if (touchingBRT == 0)
                dim.enabled = false;

            touchingBRT++;
        }

        public void TouchDIM()
        {
            if (touchingDIM == 0)
                brt.enabled = false;

            touchingDIM++;
        }

        public void UntouchBRT()
        {
            touchingBRT--;

            if (touchingBRT == 0)
                dim.enabled = true;
        }

        public void UntouchDIM()
        {
            touchingDIM--;

            if (touchingDIM == 0)
                brt.enabled = true;
        }


        private void UpdateToggled()
        {
            //Quaternion.Angle()

            float angle = Quaternion.Angle(spawnRotation, this.transform.localRotation);

            //float angle = Vector3.SignedAngle(spawnRotation.eulerAngles, this.transform.localRotation.eulerAngles, Vector3.forward);

            //Vector3 eulers = this.transform.localEulerAngles;
            //Vector3 eulers = this.transform.localRotation.eulerAngles;
            //float z = eulers.x;

            if (debug != null) {
                //debug.text = string.Format("({0}, {1}, {2})", eulers.x, eulers.y, eulers.z); 
                debug.text = string.Format("{0}", angle);
            }


            if (angle < Mathf.Abs(CurrentTarget) - switchThreshold ) {
                if (resting) {

                    // Switch the sides
                    Toggled = !Toggled;

                    cj.targetRotation = Quaternion.Euler(CurrentTarget, 0, 0);
                    resting = false;
                    Debug.Log("SWITCHED");
                }
                
            } else if (!resting) {
                resting = true;
            }

            // Handle events, sound effects
            if (Toggled && !lastToggled) {
                if (source != null && !source.isPlaying) {
                    source.pitch = upPitch;
                    source.Play();
                }

                onToggleOn.Invoke();
                if (onToggleChanged != null)
                    onToggleChanged.Invoke(true);

            } else if (!Toggled && lastToggled) {
                if (!source.isPlaying) {
                    source.pitch = downPitch;
                    source.Play();
                }

                onToggleOff.Invoke();
                if (onToggleChanged != null)
                    onToggleChanged.Invoke(false);
            }


            lastToggled = Toggled;
        }

        private float CurrentTarget
        {
            get {
                return Toggled ? low : high;
            }
        }
    }
}
