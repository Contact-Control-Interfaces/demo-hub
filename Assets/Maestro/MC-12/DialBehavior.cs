using Maestro.Vibration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Maestro
{
    public class DialBehavior : MonoBehaviour
    {
        public Rigidbody dial;
        public List<Rigidbody> toIgnore;

        public int ticks = 0;
        public int tick;
        private int lastTick;

        //public float angle = 90f;

        private ConfigurableJoint joint;

        private int register;

        private Vector3 start;
        private float offset;
        private float threshold = 0.0001f;

        private float lastAngle;

        private AudioSource dialAudio;

        private MaestroInteractable mi;
        public float tickHapticDelay = 0.01f;
        public VibrationEffect tickHapticEffect = new DoubleSharpTick(TickDuration.Short, NarrowThreeOptions._100); //new SharpClick(WideThreeOptions._60);
        private VibrationEffect originalEffect;
        private float timeElapsedSinceTick;

        List<FingerCollider> FCs;

        void Start()
        {
            // Init Audio Sources

            dialAudio = dial.gameObject.AddComponent<AudioSource>();
            dialAudio.playOnAwake = false;
            dialAudio.volume = 0.2f;
            dialAudio.pitch = 1.0f;

            // Load Audio Clips
            dialAudio.clip = Resources.Load<AudioClip>("Sounds/tumbler");

            // Init physics components
            foreach (Rigidbody ignore in toIgnore)
                Physics.IgnoreCollision(dial.gameObject.GetComponent<Collider>(), ignore.gameObject.GetComponent<Collider>(), true);


            joint = dial.gameObject.GetComponent<ConfigurableJoint>();

            // Init events
            register = 0;

            // Init dial behavior
            start = dial.transform.localPosition;
            offset = joint.linearLimit.limit;

            // Init haptics stuff
            mi = dial.gameObject.GetComponent<MaestroInteractable>();
            originalEffect = mi.haptics.Vibration;
            FCs = new List<FingerCollider>();
        }

        private float getNearestTick(float angle, int ticks, out int tick)
        {
            float delta = 360f / ticks;

            float currentTick = angle / delta;

            tick = Mathf.RoundToInt(currentTick) % ticks;

            return tick * delta;
        }

        private void Update()
        {
            if (ticks > 0) {
                float angle = dial.transform.localRotation.eulerAngles.y;

                getNearestTick(angle, ticks, out tick); //Don't set target rotation unless we want it to snap to positions

                if (lastTick != tick) {
                    dialAudio.Play();

                    // Trigger haptics
                    timeElapsedSinceTick = 0f;
                    mi.haptics.Vibration = tickHapticEffect;


                } else {
                    timeElapsedSinceTick += Time.deltaTime;
                    if (timeElapsedSinceTick > tickHapticDelay) {
                        mi.haptics.Vibration = originalEffect;
                        timeElapsedSinceTick = -1f;
                    }
                }

                lastAngle = angle;
                lastTick = tick;
            }
        }

        public void Register(FingerCollider fc)
        {
            FCs.Add(fc);

            register++;

            if (joint)
                joint.angularXMotion = ConfigurableJointMotion.Free;
        }

        public void Unregister(FingerCollider fc)
        {
            register--;
            FCs.Remove(fc);
        }
    }
}
