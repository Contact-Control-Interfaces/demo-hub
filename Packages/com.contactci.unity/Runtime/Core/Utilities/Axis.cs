using UnityEngine;

namespace Maestro
{
    public enum Axis
    {
        X, Y, Z,
        [InspectorName("-X")]
        NegX,
        [InspectorName("-Y")]
        NegY,
        [InspectorName("-Z")]
        NegZ
    }

    public static class AxisUtils
    {
        public static Vector3 GetPlaneNormal(Axis axis, Axis secondaryAxis)
            => Vector3.Cross(GetAxisVector(axis), GetAxisVector(secondaryAxis));

        public static bool IsNegative(Axis axis)
        {
            return axis == Axis.NegX || axis == Axis.NegY || axis == Axis.NegZ;
        }

        public static Vector3 GetAxisVector(Axis axis)
        {
            switch (axis) {
                default:
                case Axis.X: return Vector3.right;
                case Axis.Y: return Vector3.up;
                case Axis.Z: return Vector3.forward;
                case Axis.NegX: return -GetAxisVector(Axis.X);
                case Axis.NegY: return -GetAxisVector(Axis.Y);
                case Axis.NegZ: return -GetAxisVector(Axis.Z);
            }
        }
        
        public static float GetAxisValue(Vector3 vector, Axis axis)
        {
            switch (axis) {
                default:
                case Axis.X: return vector.x;
                case Axis.Y: return vector.y;
                case Axis.Z: return vector.z;
                case Axis.NegX: return -GetAxisValue(vector, Axis.X);
                case Axis.NegY: return -GetAxisValue(vector, Axis.Y);
                case Axis.NegZ: return -GetAxisValue(vector, Axis.Z);
            }
        }
        
        public static float GetAxisRotation(Transform transform, Axis axis)
        {
            return GetAxisValue(transform.localRotation.eulerAngles, axis);
        }
        
        public static Vector3 SetAxisValue(Vector3 vector, Axis axis, float value)
        {
            switch (axis) {
                default:
                case Axis.X: vector.x = value; break;
                case Axis.Y: vector.y = value; break;
                case Axis.Z: vector.z = value; break;
                case Axis.NegX: vector.x = -value; break;
                case Axis.NegY: vector.y = -value; break;
                case Axis.NegZ: vector.z = -value; break;
            }
            return vector;
        }
        
        public static void SetAxisRotation(Transform transform, Axis axis, float value)
        {
            transform.localRotation = Quaternion.Euler(AxisUtils.SetAxisValue(transform.localRotation.eulerAngles, axis, value));
        }
    }
}