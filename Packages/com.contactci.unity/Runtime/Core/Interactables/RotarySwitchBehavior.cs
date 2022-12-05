using Maestro.Vibration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Maestro
{
    [Serializable]
    public class RotaryEvent : UnityEvent<RotaryPosition> { }

    [Serializable]
    public struct RotaryPosition : IComparable<RotaryPosition>
    {
        public float angle;
        public string label;

        public int CompareTo(RotaryPosition other)
        {
            if (angle == other.angle) {
                return 0;
            } else if (angle < other.angle) {
                return -1;
            } else {
                return 1;
            }
        }
    }

    public class RotarySwitchBehavior : RotatingControl
    {
        public RotaryPosition[] positions;
        public string initialPosition = "OFF";
        public bool ForceInitialPosition = false;

        public GameObject Marker;

        [Space]
        public float pitch = 1f;
        public float pitchVariance = 0.1f;

        [Space]
        public AudioClip tickSound;
        private AudioSource source;

        [Space]
        [Header("Events")]
        public RotaryEvent onPositionChanged;

        [Space]
        [Header("Haptics")]
        public MaestroInteractable Interactable;
        public HapticEffect switchHaptics;

        private int lastPosition;

        public override float TargetAngle => positions[Position].angle;

        public int Position {
            get {
                for (int i = 1; i < positions.Length; i++) {
                    float midpoint_angle = (positions[i - 1].angle + positions[i].angle) / 2;
                    if (LocalAngle < midpoint_angle) {
                        return i - 1;
                    }
                }
                return positions.Length - 1;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            source = Pivot.gameObject.GetOrMake<AudioSource>(InitAudioSource);
            Interactable = Pivot.gameObject.GetOrMake<MaestroInteractable>(it => {
                it.type = InteractionType.Static;
            });

            // Sort positions by angle
            List<RotaryPosition> angles = new List<RotaryPosition>();
            angles.AddRange(positions);
            angles.Sort();
            positions = angles.ToArray();

            // Set min/max
            angleMin = -positions[positions.Length - 1].angle;
            angleMax = -positions[0].angle;

            // Mark the positions using an example marker at 0
            if (Marker != null) {
                foreach (RotaryPosition rp in positions) {
                    Vector3 rotation = AxisUtils.SetAxisValue(Vector3.zero, rotationAxis, rp.angle);

                    GameObject newMarker = Instantiate(Marker, Marker.transform.parent);
                    newMarker.transform.localPosition = Vector3.zero;
                    newMarker.transform.localRotation = Quaternion.Euler(rotation);

                    newMarker.name = $"{Marker.name} {rp.label}";
                }

                // Get rid of original marker
                Marker.SetActive(false);
                Destroy(Marker);
            }
        }

        protected override void Start()
        {
            base.Start();

            if (ForceInitialPosition) {
                ForcePosition(initialPosition, this.gameObject.name);
            }
        }

        public void ForcePosition(string position, string source)
        {
            var result = FindPosition(position);
            if (result.HasValue) {
                ForcePosition(result.Value);
            } else {
                Debug.LogError($"{source} couldn't map position {position}!");
            }
        }

        private void ForcePosition(RotaryPosition position)
        {
            LocalAngle = position.angle;
            //onPositionChanged.Invoke(position);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (Position != lastPosition) {
                PlayTickSound();
                if (onPositionChanged != null) {
                    onPositionChanged.Invoke(positions[Position]);
                }
                Interactable.SetHapticOverride(switchHaptics);
            }

            lastPosition = Position;
        }

        private RotaryPosition? FindPosition(string label)
        {
            for (int i = 0; i < positions.Length; i++) {
                if (positions[i].label.Trim().Equals(label.Trim()))
                    return positions[i];
            }
            return null;
        }

        private void PlayTickSound()
        {
            if (source.isPlaying)
                source.Stop();

            source.pitch = UnityEngine.Random.Range(pitch - pitchVariance, pitch + pitchVariance);
            source.Play();
        }

        protected void InitAudioSource(AudioSource audio)
        {
            audio.volume = 0.25f;
            audio.playOnAwake = false;

            if (tickSound != null) {
                audio.clip = tickSound;
            } else {
                audio.clip = Resources.Load<AudioClip>("Sounds/plastic_rotary");
            }
        }
    }
}
