using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public class PhysicsGrabManagerSettings : IGrabManagerSettings<PhysicsGrabManager>
    {
        [Tooltip("Once grab condition is no longer met, how long do we wait before dropping?")]
        public float WaitToRelease = 0.15f;

        [Tooltip("How aggressively do we lerp the grab anchor towards the centroid? Values >0 pull the object into the hand"), Range(0.0f, 1.0f)]
        public float CentroidLerp = 0.0f;

        [Tooltip("How much do we dampen the held object's velocity every frame? 0 does nothing, 1 slows the object to a stop"), Range(0.0f, 1.0f)]
        public float FollowForceVelocityDamper = 0.5f;

        [Tooltip("Scale for how much velocity is applied to the held object every frame. This is on top of being scaled down by Time.fixedDeltaTime")]
        public float FollowForceVelocityScalar = 750;

        [Tooltip("Display debug text on corresponding hand panel?")]
        public bool Debug = false;

        protected override void ApplySettings(PhysicsGrabManager manager)
        {
            manager.WaitToRelease = WaitToRelease;
            manager.CentroidLerp = CentroidLerp;
            manager.FollowForceVelocityDamper = FollowForceVelocityDamper;
            manager.FollowForceVelocityScalar = FollowForceVelocityScalar;
            manager.debug = Debug;
        }
    }
}
