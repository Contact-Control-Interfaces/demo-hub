using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public class PositionSetter : MonoBehaviour
    {
        public Transform Tracker;

        public Transform Origin;

        public bool RequireBothHands = false;

        private Vector3? left;
        private Vector3? right;

        public void LateUpdate()
        {
            if (left != null && right != null) {
                // Find rotation between the two
                Vector3 rightTarget = right.Value;
                Vector3 leftTarget = LeftPositionFromOffset(left.Value - Origin.position);

                Tracker.position = (rightTarget + leftTarget) / 2;
            } else if (!RequireBothHands) {
                if (left != null && Origin != null) {
                    Vector3 trackPos = LeftPositionFromOffset(left.Value - Origin.position);

                    Tracker.position = trackPos;
                } else if (right != null) {
                    Tracker.position = right.Value;
                }
            }

            left = null;
            right = null;
        }

        private Vector3 LeftPositionFromOffset(Vector3 rightOffset)
        {
            return Vector3.Scale(rightOffset, new Vector3(1, -1, -1)) + Origin.position;
        }

        public void SetRightPosition(Vector3 position)
        {
            right = position;
        }

        public void SetLeftPosition(Vector3 position)
        {
            left = position;
        }
    }
}
