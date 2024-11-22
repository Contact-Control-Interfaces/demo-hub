using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Maestro
{
    [System.Serializable]
    public class IndustrialToggleEvent : UnityEvent<IndustrialToggleState> { }

    public enum IndustrialToggleState
    {
        On, Off, Neutral
    };

    public class IndustrialSwitchBehavior : RotatingControl
    {
        public override float TargetAngle {
            get {
                if (threeState && state == IndustrialToggleState.Neutral) {
                    return 0;
                } else {
                    return state == IndustrialToggleState.On ? -extent : extent;
                }
            }
        }

        [Space]
        public IndustrialToggleState initialState;
        public IndustrialToggleState state;
        private IndustrialToggleState lastState;
        public float extent = 27;

        [Space]
        public bool threeState;
        public float innerExtent = 15;

        [Space]
        public AudioClip toggleSound;
        public float pitchMin = 0.95f;
        public float pitchMax = 1.05f;

        [Space]
        [Header("Events")]
        public IndustrialToggleEvent onToggleChanged;
        public UnityEvent onToggleOn, onToggleOff, onToggleNeutral;

        private AudioSource source;

        public Rigidbody connectedObject;

        protected override void Awake()
        {
            base.Awake();

            angleMin = -extent;
            angleMax = extent;

            if (onToggleOn == null)
                onToggleOn = new UnityEvent();

            if (onToggleOff == null)
                onToggleOff = new UnityEvent();

            if (onToggleNeutral == null)
                onToggleNeutral = new UnityEvent();

            source = Pivot.gameObject.GetOrMake<AudioSource>(InitAudioSource);
        }

        protected override void Start()
        {
            base.Start();

            // Move switch to initial position
            state = initialState;
            LocalAngle = TargetAngle;

            // Call event for initial state
            CallToggleEvent(state);
        }

        protected override void FixedUpdate()
        {
            // Update state
            if (threeState && Mathf.Abs(LocalAngle) < innerExtent) {
                state = IndustrialToggleState.Neutral;
            }
            else if (LocalAngle <= angleMin)
            { 
                state = IndustrialToggleState.On;
            }
            else if(LocalAngle >= angleMax)
            {
                state = IndustrialToggleState.Off;
            }

            base.FixedUpdate();

            // Call events
            if (state != lastState) {
                CallToggleEvent(state);
                PlayToggleSound();
            }

            lastState = state;
        }

        protected override void InitJoint(ConfigurableJoint cj)
        {
            base.InitJoint(cj);

            cj.projectionMode = JointProjectionMode.PositionAndRotation;
            cj.projectionDistance = 0.0001f;
            cj.projectionAngle = 1f;
            cj.connectedBody = connectedObject;
        }

        private void PlayToggleSound()
        {
            if (source.isPlaying)
                source.Stop();

            source.pitch = Random.Range(pitchMin, pitchMax);
            source.Play();
        }

        protected void InitAudioSource(AudioSource audio)
        {
            audio.volume = 0.5f;
            audio.playOnAwake = false;

            if (toggleSound != null) {
                audio.clip = toggleSound;
            } else {
                audio.clip = Resources.Load<AudioClip>("Sounds/switch");
            }
        }

        private void CallToggleEvent(IndustrialToggleState state)
        {
            switch (state) {
                case IndustrialToggleState.On: OnToggledOn(); break;
                case IndustrialToggleState.Off: OnToggledOff(); break;
                case IndustrialToggleState.Neutral: OnToggledNeutral(); break;
            }
        }

        private void OnToggledOn()
        {
            onToggleOn.Invoke();
            if (onToggleChanged != null)
                onToggleChanged.Invoke(IndustrialToggleState.On);
        }

        private void OnToggledOff()
        {
            onToggleOff.Invoke();
            if (onToggleChanged != null)
                onToggleChanged.Invoke(IndustrialToggleState.Off);
        }

        public void OnToggledNeutral()
        {
            onToggleNeutral.Invoke();
            if (onToggleChanged != null)
                onToggleChanged.Invoke(IndustrialToggleState.Neutral);
        }
    }
}
