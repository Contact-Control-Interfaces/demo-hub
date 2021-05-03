using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public class NailBehavior : MonoBehaviour
    {

        private float totalDepth = 0f;

        public bool detached = false;

        //public float maxDepth = 

        public void Nail(Vector3 relativeVelocity)
        {
            float depth = Mathf.Max(relativeVelocity.y / 150f, 0f);
            totalDepth += depth;

            this.transform.Translate(0f, depth, 0f, this.transform);
            totalDepth += depth;


        }

        public void Elongate()
        {
            CapsuleCollider cc = this.GetComponent<CapsuleCollider>();

            cc.height = 10;
            cc.center = new Vector3(cc.center.x, cc.center.y + 4, cc.center.z);

            this.GetComponent<Rigidbody>().useGravity = true;

            this.transform.parent = null;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.GetComponent<HammerResponse>() || collision.gameObject.GetComponent<FingerCollider>())
                Nail(collision.relativeVelocity);
            else
                this.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }
}
