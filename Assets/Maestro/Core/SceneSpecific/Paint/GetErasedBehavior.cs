using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetErasedBehavior : MonoBehaviour
{
    /*void OnTriggerEnter(Collider other)
    {
        FingerTipCollider ftc = other.gameObject.GetComponent<FingerTipCollider>();
        if (ftc != null && ftc.PaintColor == FingerPaint.eraseColor)
        {

            Debug.Log("ERASED");
            Destroy(this.gameObject);
        }
    }*/

    void OnCollisionEnter(Collision collision)
    {
        TryErase(collision.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        TryErase(other.transform.parent.gameObject);
    }

    private void TryErase(GameObject go)
    {
        if (go != null)
        {
            FingerTipCollider ftc = go.gameObject.GetComponent<FingerTipCollider>();
            if (ftc != null && ftc.isTip && ftc.PaintColor == FingerPaint.eraseColor)
            {
                //Debug.Log("ERASED");
                Destroy(this.gameObject);
            }
        }
    }
}
