using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public class CanoeBehavior : MonoBehaviour
    {
        public WaterGenerator water;

        public float dx, dy, dz;

        // Start is called before the first frame update
        void Start()
        {
            MoveToUser();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                MoveToUser();
            else
                ReactToWater();
        }

        private void ReactToWater()
        {
            this.transform.Translate(-Vector3.up * (water.WaterLevel(this.transform.position) + this.transform.position.y));
        }

        private void MoveToUser()
        {
            Camera cam = GameObject.FindObjectOfType<Camera>();

            this.transform.position = Vector3.Scale(cam.transform.position + new Vector3(dx, dy, dz), new Vector3(1, 0, 1));
            this.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(cam.transform.forward, new Vector3(0, 1, 0)));

            ReactToWater();
        }
    }
}
