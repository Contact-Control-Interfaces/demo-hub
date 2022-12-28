using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public interface IPoseApplicator
    {
        Vector3 CurrentPosition
        {
            get;
        }

        Quaternion CurrentRotation
        {
            get;
        }

        MaestroHand MaestroHand
        {
            get;
        }

        Poser.HandBones EnforcePose(Poser.HandBones targets, float?[] clamps, bool force = false);

        void EnforcePosition(Transform t);

        Vector3 GetCurrentRotation(Poser.HandBones target);

        float GetCurrentCurl(Poser.HandBones target);
    }
}