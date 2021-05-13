using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public class MaestroManager : MonoBehaviour
    {
        private MaestroHandV2 leftHand;
        private MaestroHandV2 rightHand;

        public MaestroHandV2 LeftHand
        {
            get
            {
                return leftHand;
            }
            set{
                leftHand = value;
                if (leftHand)
                    leftHand.manager = this;
            }
        }
        public MaestroHandV2 RightHand
        {
            get
            {
                return rightHand;
            }
            set
            {
                rightHand = value;
                if (rightHand)
                    rightHand.manager = this;
            }
        }

        public bool InteractablesOnly = false;
        public HapticEffect DefaultEffect = new HapticEffect { Amplitude = 115, Vibration = 3 };
        public float TipSize;
        public float MiddleSize;
        public float KnuckleSize;
        public FlatnessChecker flatnessChecker;
        public LayerMask objectLayer;
        public float palmMeshWait = 0.1f;
        public float tooClose = 0.1f;
        public float tooFast = 0.2f;

        public void cascadeProperties()
        {
            if (LeftHand != null && !LeftHand.settingsOverride)
            {
                LeftHand.interactablesOnly = InteractablesOnly;
                LeftHand.defaultEffect = DefaultEffect;
                LeftHand.TipSize = TipSize;
                LeftHand.MiddleSize = MiddleSize;
                LeftHand.KnuckleSize = KnuckleSize;
                LeftHand.flatnessChecker = flatnessChecker;
                LeftHand.objectLayer = objectLayer;
                LeftHand.palmMeshWait = palmMeshWait;
                LeftHand.tooClose = tooClose;
                LeftHand.tooFast = tooFast;
                LeftHand.otherHand = RightHand;
                LeftHand.whichHand = WhichHand.LeftHand;
            }
            if (RightHand != null && !RightHand.settingsOverride)
            {
                RightHand.interactablesOnly = InteractablesOnly;
                RightHand.defaultEffect = DefaultEffect;
                RightHand.TipSize = TipSize;
                RightHand.MiddleSize = MiddleSize;
                RightHand.KnuckleSize = KnuckleSize;
                RightHand.flatnessChecker = flatnessChecker;
                RightHand.objectLayer = objectLayer;
                RightHand.palmMeshWait = palmMeshWait;
                RightHand.tooClose = tooClose;
                RightHand.tooFast = tooFast;
                RightHand.otherHand = LeftHand;
                RightHand.whichHand = WhichHand.RightHand;
            }
        }

        public byte? Amplitude {
            get {
                if (activeHaptics.Count > 0)
                    return activeHaptics.Max.Amplitude;
                else return null;
            }
        }
        public byte? VibrationEffect {
            get {
                if (activeHaptics.Count > 0)
                    return activeHaptics.Max.VibrationEffect;
                else return null;
            }
        }


        private static float DefaultDuration = 0.3125f; //0.25f

        private SortedSet<MaestroGlobalHapticEffect> activeHaptics = new SortedSet<MaestroGlobalHapticEffect>();

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            activeHaptics.RemoveWhere(x => !x.isActive());
        }

        public void SetHaptics(byte amplitude, byte effect, float duration)
        {
            activeHaptics.Add(new MaestroGlobalHapticEffect(amplitude, effect, duration));
        }

        public void PlayEffect(int effect)
        {
            SetHaptics(0, (byte)effect, DefaultDuration);
        }
    }

    class MaestroGlobalHapticEffect
    {
        public int Priority;
        public byte? Amplitude;
        public byte? VibrationEffect;
        private float Duration;
        private float StartTime;

        public MaestroGlobalHapticEffect(int priority, byte? amplitude, byte? vibrationEffect, float duration)
        {
            this.Priority = priority;
            this.Amplitude = amplitude;
            this.VibrationEffect = vibrationEffect;
            this.Duration = duration;
            StartTime = Time.time;
        }

        public MaestroGlobalHapticEffect(byte? amplitude, byte? vibrationEffect, float duration)
            : this(0, amplitude, vibrationEffect, duration) { }

        public MaestroGlobalHapticEffect(byte? amplitude, byte? vibrationEffect)
            : this(amplitude, vibrationEffect, -1f) { }

        public bool isActive()
        {
            return Duration < 0 || Time.time < StartTime + Duration;
        }
    }

    class MaestroGlobalHapticEffectComparer : IComparer<MaestroGlobalHapticEffect>
    {
        public int Compare(MaestroGlobalHapticEffect a, MaestroGlobalHapticEffect b)
        {
            if (a.Priority != b.Priority) {
                return a.Priority.CompareTo(b.Priority);
            } else {
                return a.VibrationEffect.GetValueOrDefault(0).CompareTo(b.VibrationEffect.GetValueOrDefault(0));
            }
        }
    }
}
