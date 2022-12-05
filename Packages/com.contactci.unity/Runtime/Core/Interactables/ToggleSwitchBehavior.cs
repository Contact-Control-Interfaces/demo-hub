using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Maestro
{
    [System.Serializable]
    public class ToggleEvent : UnityEvent<ToggleState> { }

    public enum ToggleState
    {
        On, Off, Neutral
    };

    public class ToggleSwitchBehavior : RotatingControl
    {
        public override float TargetAngle {
            get {
                if (threeState && state == ToggleState.Neutral) {
                    return 0;
                } else {
                    return state == ToggleState.On ? -extent : extent;
                }
            }
        }

        [Space]
        public ToggleState initialState;
        public ToggleState state;
        private ToggleState lastState;
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
        public ToggleEvent onToggleChanged;
        public UnityEvent onToggleOn, onToggleOff, onToggleNeutral;

        private AudioSource source;

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
                state = ToggleState.Neutral;
            } else {
                state = LocalAngle < 0 ? ToggleState.On : ToggleState.Off;
            }

            base.FixedUpdate();

            // Call events
            if (state != lastState) {
                CallToggleEvent(state);
                PlayToggleSound();
            }

            lastState = state;
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

        private void CallToggleEvent(ToggleState state)
        {
            switch (state) {
                case ToggleState.On: OnToggledOn(); break;
                case ToggleState.Off: OnToggledOff(); break;
                case ToggleState.Neutral: OnToggledNeutral(); break;
            }
        }

        private void OnToggledOn()
        {
            onToggleOn.Invoke();
            if (onToggleChanged != null)
                onToggleChanged.Invoke(ToggleState.On);
        }

        private void OnToggledOff()
        {
            onToggleOff.Invoke();
            if (onToggleChanged != null)
                onToggleChanged.Invoke(ToggleState.Off);
        }

        private void OnToggledNeutral()
        {
            onToggleNeutral.Invoke();
            if (onToggleChanged != null)
                onToggleChanged.Invoke(ToggleState.Neutral);
        }
    }
}
