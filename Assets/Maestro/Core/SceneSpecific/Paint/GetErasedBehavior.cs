using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public class GetErasedBehavior : MonoBehaviour
    {

        void OnCollisionEnter(Collision collision)
        {
            TryErase(collision.gameObject);
        }

        void OnTriggerEnter(Collider other)
        {
            //TryErase(other.transform.parent.gameObject);
            TryErase(other.transform.gameObject);
        }

        private void TryErase(GameObject go)
        {
            if (go != null) {
                FingerCollider fc = go.gameObject.GetComponent<FingerCollider>();
                if (fc != null && fc.isTip && fc.transform.Find("Paint") != null && fc.transform.Find("Paint").gameObject.GetComponent<PaintType>().erase) {
                    Destroy(this.gameObject);
                }
            }
        }
    }
}
