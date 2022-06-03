using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public class CylinderBetween : MonoBehaviour
    {
        public Transform a;
        public Transform b;
        public float size;

        private float scale = 0.005f;

        private Rigidbody _ar;
        private Rigidbody ar {
            get {
                if (!_ar) _ar = a.GetComponent<Rigidbody>();
                return _ar;
            }
        }

        private Rigidbody _br;
        private Rigidbody br {
            get {
                if (!_br) _br = b.GetComponent<Rigidbody>();
                return _br;
            }
        }

        private Rigidbody rb;

        // Start is called before the first frame update
        void Start()
        {
            rb = this.GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            if (a && b) {
                Vector3 start = transform.position;
                Vector3 end = (a.position + b.position) / 2;

                transform.position = end;
                if (rb != null)
                    rb.velocity = (end - start) / Time.deltaTime;

                transform.localScale = new Vector3(size, (b.position - a.position).magnitude / 2, size);
                Vector3 diff = b.position - a.position;
                if (diff.magnitude > 0) {
                    transform.rotation = Quaternion.LookRotation(Vector3.Cross(diff, Vector3.forward), diff);
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (ar)
                ar.AddForce(-collision.impulse * scale, ForceMode.Impulse);
            if (br)
                br.AddForce(-collision.impulse * scale, ForceMode.Impulse);
        }

        private void OnCollisionStay(Collision collision)
        {
            if (ar)
                ar.AddForce(-collision.impulse * scale, ForceMode.Impulse);

            if (br)
                br.AddForce(-collision.impulse * scale, ForceMode.Impulse);
        }
    }
}