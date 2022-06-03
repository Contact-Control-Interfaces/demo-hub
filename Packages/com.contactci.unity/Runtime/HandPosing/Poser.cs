using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Maestro
{
    public class Poser : MonoBehaviour
    {
        [Flags]
        public enum HandBones
        {
            ThumbProximal   = 1 << 0,
            ThumbMiddle     = 1 << 1,
            ThumbDistal     = 1 << 2,
            IndexProximal   = 1 << 3,
            IndexMiddle     = 1 << 4,
            IndexDistal     = 1 << 5,
            MiddleProximal  = 1 << 6,
            MiddleMiddle    = 1 << 7,
            MiddleDistal    = 1 << 8,
            RingProximal    = 1 << 9,
            RingMiddle      = 1 << 10,
            RingDistal      = 1 << 11,
            LittleProximal  = 1 << 12,
            LittleMiddle    = 1 << 13,
            LittleDistal    = 1 << 14
        };
        public static HandBones[] AllBones = (HandBones[]) Enum.GetValues(typeof(HandBones));
        public static readonly int BoneCount = AllBones.Length;

        public bool active = false;
        public HandBones targetedBones;

        public MonoBehaviour provider;
        protected IPoseApplicator _provider;

        protected HandBones lastClamped;

        [Space]
        [Header("Clamp values")]
        public float defaultClamp = 45f;
        public float[] clamps;

        [Space]
        public bool preview;

        protected virtual void Start()
        {
            _provider = provider as IPoseApplicator;
            if (_provider == null)
                Debug.LogError("Couldn't find pose provider!");

            if (clamps == null || clamps.Length < AllBones.Length)
                clamps = Enumerable.Repeat(defaultClamp, AllBones.Length).ToArray();
        }

        protected virtual void LateUpdate()
        {
            if (active && _provider != null) {
                lastClamped = _provider.EnforcePose(GetTargetMask(), 
                    clamps.Select<float, float?>(x => x < 0 ? (float?)null : x).ToArray(), preview);
            }
        }

        protected virtual HandBones GetTargetMask()
        {
            return targetedBones;
        }
    }
}
