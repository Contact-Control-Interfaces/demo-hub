using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public class HoopMoverV2 : MonoBehaviour
    {
        public Transform hoop, start, far, top;
        public float bounds, offset;

        private Vector3 startLocalScale;
        private Quaternion startLocalRotation;

        private float startZ;

        void Start()
        {
            startZ = this.transform.localPosition.z;
            GetStartingValues();
        }

        private void GetStartingValues()
        {
            startLocalScale = this.transform.localScale;
            startLocalRotation = this.transform.localRotation;
        }

        private void ApplyStartingValues()
        {
            this.transform.localScale = startLocalScale;
            this.transform.localRotation = startLocalRotation;
        }

        void Update()
        {
            //Move handle back in bounds
            Vector3 localPosition = this.transform.localPosition;
            if (localPosition.x < -bounds)
                localPosition.x = -bounds;
            else if (localPosition.x > bounds)
                localPosition.x = bounds;

            if (localPosition.y < -bounds)
                localPosition.y = -bounds;
            else if (localPosition.y > bounds - offset)
                localPosition.y = bounds - offset;

            localPosition.z = startZ;

            this.transform.localPosition = localPosition;
            ApplyStartingValues();

            //Move hoop
            float toFro = (localPosition.x + bounds) / (bounds * 2);
            float upDown = (localPosition.y + bounds) / (bounds * 2 - offset);

            hoop.transform.position = Vector3.Lerp(start.position, far.position, 1 - toFro) + (upDown * (top.position - far.position));
        }
    }
}