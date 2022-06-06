using System;
using UnityEngine;

namespace Maestro
{
    public class PoseApplicator : MonoBehaviour, IPoseApplicator
    {
        public MaestroHand maestroHand;
        
        public Vector3 CurrentPosition => maestroHand.mc.PalmContainer.position;

        public Quaternion CurrentRotation => maestroHand.mc.PalmContainer.rotation;

        public MaestroHand MaestroHand => maestroHand;

        public Poser.HandBones EnforcePose(Poser.HandBones targets, float?[] clamps, bool force = false)
        {
            Poser.HandBones clamped = 0;

            for (int i = 0; i < Poser.BoneCount; i++) {
                if (targets.HasFlag(Poser.AllBones[i])) {
                    PointOnHand poh = maestroHand.mc[getMaestroIndex(Poser.AllBones[i])];

                    Vector3 clampedEulers = poh.transform.localRotation.eulerAngles;
                    if (clamps[i].HasValue) {
                        float z = clampedEulers.z;
                        if (z > 180)
                            z = z - 360;

                        // Assume -Z rotation is the curl direction
                        if (z < -clamps[i].Value || force) {
                            z = -clamps[i].Value;
                            clamped |= (Poser.HandBones)(1 << i);
                        }

                        clampedEulers.z = z;
                        poh.transform.localRotation = Quaternion.Euler(clampedEulers);
                    }
                }
            }

            return clamped;
        }

        public void EnforcePosition(Transform t)
        {
            //maestroHand.mc.PalmBase.transform.position = t.position;
            //maestroHand.mc.PalmBase.transform.rotation = t.rotation;
        }
        
        protected MaestroIndex getMaestroIndex(Poser.HandBones bone)
        {
            switch (bone) {
                default: throw new NotImplementedException(string.Format("Bone {0} not supported!", bone.ToString()));

                case Poser.HandBones.ThumbProximal: return new MaestroIndex(WhichFinger.Thumb, PointOnFinger.Base);
                case Poser.HandBones.ThumbMiddle:   return new MaestroIndex(WhichFinger.Thumb, PointOnFinger.Middle);
                case Poser.HandBones.ThumbDistal:   return new MaestroIndex(WhichFinger.Thumb, PointOnFinger.Tip);

                case Poser.HandBones.IndexProximal: return new MaestroIndex(WhichFinger.Index, PointOnFinger.Base);
                case Poser.HandBones.IndexMiddle:   return new MaestroIndex(WhichFinger.Index, PointOnFinger.Middle);
                case Poser.HandBones.IndexDistal:   return new MaestroIndex(WhichFinger.Index, PointOnFinger.Tip);


                case Poser.HandBones.MiddleProximal: return new MaestroIndex(WhichFinger.Middle, PointOnFinger.Base);
                case Poser.HandBones.MiddleMiddle:   return new MaestroIndex(WhichFinger.Middle, PointOnFinger.Middle);
                case Poser.HandBones.MiddleDistal:   return new MaestroIndex(WhichFinger.Middle, PointOnFinger.Tip);

                case Poser.HandBones.RingProximal: return new MaestroIndex(WhichFinger.Ring, PointOnFinger.Base);
                case Poser.HandBones.RingMiddle:   return new MaestroIndex(WhichFinger.Ring, PointOnFinger.Middle);
                case Poser.HandBones.RingDistal:   return new MaestroIndex(WhichFinger.Ring, PointOnFinger.Tip);

                case Poser.HandBones.LittleProximal: return new MaestroIndex(WhichFinger.Little, PointOnFinger.Base);
                case Poser.HandBones.LittleMiddle:   return new MaestroIndex(WhichFinger.Little, PointOnFinger.Middle);
                case Poser.HandBones.LittleDistal:   return new MaestroIndex(WhichFinger.Little, PointOnFinger.Tip);
            }
        }
    }
}
