using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Maestro
{
    [System.Serializable]
    public class ToggleEvent : UnityEvent<bool> { }

    public enum Axis
    {
        X, Y, Z,
        [InspectorName("-X")]
        NegX,
        [InspectorName("-Y")]
        NegY,
        [InspectorName("-Z")]
        NegZ
    };

    public class ToggleSwitchBehavior : MonoBehaviour
    {
        public bool Toggled;

        [Space]
        [Header("Config")]
        public float extent = 27f;
        public bool initialState = false;
        public Axis rotationAxis = Axis.Z;
        public Axis secondaryAxis = Axis.X;
        public bool invert;
        public bool fireEventsForInitialState;

        [Space]
        public float slerpSpring = 5f;
        public float slerpDamper = 0.1f;

        [Space]
        public AudioClip toggleSound;
        public float pitchMin = 0.95f;
        public float pitchMax = 1.05f;

        [Space]
        [Header("Events")]
        public ToggleEvent onToggleChanged;
        public UnityEvent onToggleOn, onToggleOff;

        [Space]
        public List<Collider> toIgnore;
        private Collider[] colliders;

        private ConfigurableJoint cj;
        private AudioSource source;

        private bool lastToggled = false;

        private bool IsNeg(Axis axis)
        {
            return axis == Axis.NegX || axis == Axis.NegY || axis == Axis.NegZ;
        }

        private float LocalEulerAngle {
            get {
                float result;

                switch (rotationAxis) {
                    default:
                    case Axis.X: result = this.transform.localRotation.eulerAngles.x; break;
                    case Axis.Y: result = this.transform.localRotation.eulerAngles.y; break;
                    case Axis.Z: result = this.transform.localRotation.eulerAngles.z; break;
                }

                // keep the angles between -180 to 180
                return result > 180 ? result - 360 : result;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            source = GetComponent<AudioSource>();
            if (source == null)
                source = this.gameObject.AddComponent<AudioSource>();

            InitAudioSource();

            if (onToggleOn == null)
                onToggleOn = new UnityEvent();

            if (onToggleOff == null)
                onToggleOff = new UnityEvent();

            cj = this.GetComponent<ConfigurableJoint>();
            if (cj == null)
                cj = this.gameObject.AddComponent<ConfigurableJoint>();

            InitJoint();

            // Do ignores
            colliders = this.GetComponentsInChildren<Collider>();
            if (toIgnore != null && toIgnore.Count > 0) {
                foreach (Collider ignored in toIgnore) {
                    foreach (Collider c in colliders) {
                        Physics.IgnoreCollision(ignored, c);
                    }
                }
            }

            // Move switch to start position
            this.transform.localRotation = getTargetRotation(initialState);

            Toggled = getToggleState();

            if (fireEventsForInitialState) {
                if (Toggled)
                    OnToggledOn();
                else
                    OnToggledOff();
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            UpdateToggled();
        }

        public void PlayToggleSound()
        {
            source.pitch = Random.Range(pitchMin, pitchMax);
            source.Play();
        }

        private void SetToggleRange(float angle)
        {
            SetToggleRange(-angle, angle);
        }

        private void SetToggleRange(float low, float high)
        {
            cj.highAngularXLimit = new SoftJointLimit() { limit = high };
            cj.lowAngularXLimit = new SoftJointLimit() { limit = low };
        }

        private void UpdateTargetRotation()
        {
            cj.targetRotation = Quaternion.Euler(extent * (Toggled ? 1 : -1) * (invert ? -1 : 1), 0, 0);
        }

        private Quaternion getTargetRotation()
        {
            return getTargetRotation(Toggled);
        }

        private Quaternion getTargetRotation(bool isToggled)
        {
            return Quaternion.Euler(extent * (isToggled ? 1 : -1) * (invert ? -1 : 1), 0, 0);
        }

        private bool getToggleState()
        {
            bool result = LocalEulerAngle < 0;
            if (invert) {
                result = !result;
            }
            if (IsNeg(rotationAxis)) {
                result = !result;
            }

            return result;
        }

        private void OnToggledOn()
        {
            onToggleOn.Invoke();
            if (onToggleChanged != null)
                onToggleChanged.Invoke(true);
        }

        private void OnToggledOff()
        {
            onToggleOff.Invoke();
            if (onToggleChanged != null)
                onToggleChanged.Invoke(false);
        }

        private void UpdateToggled()
        {
            Toggled = getToggleState();

            if (Toggled && !lastToggled) {
                UpdateTargetRotation();

                if (!source.isPlaying) {
                    PlayToggleSound();
                }

                OnToggledOn();

            } else if (!Toggled && lastToggled) {
                UpdateTargetRotation();

                if (!source.isPlaying) {
                    PlayToggleSound();
                }

                OnToggledOff();
            }

            lastToggled = Toggled;
        }

        private Vector3 getAxisVector(Axis axis)
        {
            Vector3 result;
            switch (axis) {
                default: result = Vector3.right; break;
                case Axis.Y: result = Vector3.up; break;
                case Axis.Z: result = Vector3.forward; break;
                case Axis.NegX: result = -Vector3.right; break;
                case Axis.NegY: result = -Vector3.up; break;
                case Axis.NegZ: result = -Vector3.forward; break;
            }

            return result;
        }

        private void InitJoint()
        {
            if (cj != null) {

                // Setup axes
                // we want x to be the main axis as far as the joint is concerned
                // since the X rotation is the only axis that has an upper/lower limit
                cj.secondaryAxis = getAxisVector(secondaryAxis);
                cj.axis = getAxisVector(rotationAxis);

                // Lock all translational movement
                cj.xMotion = ConfigurableJointMotion.Locked;
                cj.yMotion = ConfigurableJointMotion.Locked;
                cj.zMotion = ConfigurableJointMotion.Locked;

                // Lock all rotational movement but X
                cj.angularXMotion = ConfigurableJointMotion.Limited;
                cj.angularYMotion = ConfigurableJointMotion.Locked;
                cj.angularZMotion = ConfigurableJointMotion.Locked;

                // Set angular drive
                JointDrive slerp = new JointDrive { positionSpring = slerpSpring, positionDamper = slerpDamper, maximumForce = 1000000f };
                cj.rotationDriveMode = RotationDriveMode.Slerp;
                cj.slerpDrive = slerp;

                // Set main axis range
                SetToggleRange(extent);
                cj.projectionAngle = Mathf.Max(Mathf.Abs(cj.lowAngularXLimit.limit), cj.highAngularXLimit.limit);

                // Set initial target
                UpdateTargetRotation();

            } else {
                Debug.LogError("Toggle switch has no corresponding ConfigurableJoint!");
            }
        }

        private void InitAudioSource()
        {
            if (source != null) {
                source.volume = 0.5f;
                source.playOnAwake = false;

                if (toggleSound != null) {
                    source.clip = toggleSound;
                } else {
                    source.clip = Resources.Load<AudioClip>("Sounds/switch");
                }
            }
        }
    }
}