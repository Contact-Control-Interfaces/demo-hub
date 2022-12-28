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
            IndexProximal   = 1 << 2,
            IndexMiddle     = 1 << 3,
            IndexDistal     = 1 << 4,
            MiddleProximal  = 1 << 5,
            MiddleMiddle    = 1 << 6,
            MiddleDistal    = 1 << 7,
            RingProximal    = 1 << 8,
            RingMiddle      = 1 << 9,
            RingDistal      = 1 << 10,
            LittleProximal  = 1 << 11,
            LittleMiddle    = 1 << 12,
            LittleDistal    = 1 << 13
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

        public void LockCorresponding(HandBones target)
        {
            for (int i = 0; i < BoneCount; i++) {
                if (target.HasFlag(AllBones[i])) {
                    clamps[i] = -1 * _provider.GetCurrentCurl(AllBones[i]);
                }
            }
        }

        protected virtual HandBones GetTargetMask()
        {
            return targetedBones;
        }

        public static HandBones FromIndex(MaestroIndex index)
        {
            return FromFinger(index.finger);
        }

        public static HandBones FromFinger(WhichFinger finger)
        {
            return finger switch {
                WhichFinger.Index => HandBones.IndexProximal | HandBones.IndexMiddle | HandBones.IndexDistal,
                WhichFinger.Middle => HandBones.MiddleProximal | HandBones.MiddleMiddle | HandBones.MiddleDistal,
                WhichFinger.Ring => HandBones.RingProximal | HandBones.RingMiddle | HandBones.RingDistal,
                WhichFinger.Little => HandBones.LittleProximal | HandBones.LittleMiddle | HandBones.LittleDistal,
                WhichFinger.Thumb => HandBones.ThumbProximal | HandBones.ThumbMiddle,
                WhichFinger.Palm => 0,
                _ => throw new ArgumentOutOfRangeException("Couldn't convert to HandBones!")
            };
        }
    }
}
