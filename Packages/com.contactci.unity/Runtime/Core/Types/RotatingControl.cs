using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Maestro
{
    public abstract class RotatingControl : MonoBehaviour
    {
        public Transform Pivot;

        [Space]
        public Axis rotationAxis = Axis.Z;
        public Axis secondaryAxis = Axis.X;

        [Space]
        public float slerpSpring = 5f;
        public float slerpDamper = 0.1f;

        [Space]
        public float angleMin = -25f;
        public float angleMax = 25f;

        public abstract float TargetAngle { get; }
        protected float LocalAngle {
            get {
                float result = AxisUtils.GetAxisRotation(Pivot, rotationAxis);
                // keep the angles between -180 to 180
                return result > 180 ? result - 360 : result;
            }

            set {
                AxisUtils.SetAxisRotation(Pivot, rotationAxis, value);
            }
        }

        protected ConfigurableJoint joint;

        protected virtual void Awake()
        {
            if (Pivot == null)
                Pivot = this.transform;
        }

        protected virtual void Start()
        {
            joint = Pivot.gameObject.GetOrMake<ConfigurableJoint>(InitJoint);
        }

        protected virtual void FixedUpdate()
        {
            joint.targetRotation = Quaternion.Euler(-TargetAngle, 0, 0);
        }

        protected virtual void OnDrawGizmos()
        {
            if (Pivot == null)
                Pivot = this.transform;

            float size = 0.01f;

            // Show main rotation axis
            Handles.color = Color.red;
            Handles.DrawWireDisc(
                Pivot.position,
                Pivot.TransformDirection(AxisUtils.GetAxisVector(this.rotationAxis)), size);

            // Show secondary axis
            Handles.DrawLine(Pivot.position, Pivot.position + size * Pivot.TransformDirection(AxisUtils.GetAxisVector(this.secondaryAxis)));
        }

        private void InitJoint(ConfigurableJoint cj)
        {
            if (cj != null) {

                // Setup axes
                // we want x to be the main axis as far as the joint is concerned
                // since the X rotation is the only axis that has an upper/lower limit
                cj.secondaryAxis = AxisUtils.GetAxisVector(secondaryAxis);
                cj.axis = AxisUtils.GetAxisVector(rotationAxis);
                cj.anchor = Vector3.zero;

                // Lock all translational movement
                cj.xMotion = ConfigurableJointMotion.Locked;
                cj.yMotion = ConfigurableJointMotion.Locked;
                cj.zMotion = ConfigurableJointMotion.Locked;

                // Lock all rotational movement but X
                cj.angularXMotion = ConfigurableJointMotion.Limited;
                cj.angularYMotion = ConfigurableJointMotion.Locked;
                cj.angularZMotion = ConfigurableJointMotion.Locked;

                // Set angular drive
                JointDrive slerp = new JointDrive { positionSpring = slerpSpring, positionDamper = slerpDamper, maximumForce = 1000000f };
                cj.rotationDriveMode = RotationDriveMode.Slerp;
                cj.slerpDrive = slerp;

                // Set main axis range
                SetJointRange(cj, angleMin, angleMax);
                cj.projectionAngle = Mathf.Max(Mathf.Abs(cj.lowAngularXLimit.limit), cj.highAngularXLimit.limit);

            } else {
                Debug.LogError("Toggle switch has no corresponding ConfigurableJoint!");
            }
        }        

        public static void SetJointRange(ConfigurableJoint cj, float low, float high)
        {
            cj.highAngularXLimit = new SoftJointLimit() { limit = high };
            cj.lowAngularXLimit = new SoftJointLimit() { limit = low };
        }
    }
}
