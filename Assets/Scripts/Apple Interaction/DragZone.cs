using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragZone : MonoBehaviour
{
    private static int InsideHowManyZones = 0;
    private float initialDrag;

    private bool insideThisZone = false;

    public float insideDrag;

    public Collider appleCollider;
    public GameObject objectHolder;
    float initialMaxLinearVelocity;

    private void OnTriggerEnter(Collider other)
    {
        if (other == appleCollider) {
            insideThisZone = true;

            Rigidbody r = other.GetComponent<Rigidbody>();
            if (r != null) {
                if (InsideHowManyZones == 0) {
                    initialDrag = r.drag;
                    initialMaxLinearVelocity = r.maxLinearVelocity;
                    ApplyDrag(r);
                }

                InsideHowManyZones++;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == appleCollider) {
            insideThisZone = false;

            Rigidbody r = other.GetComponent<Rigidbody>();
            if (r != null) {
                InsideHowManyZones--;

                if (InsideHowManyZones == 0) {
                    UnapplyDrag(r);
                }
            }
        }
    }

    private void ApplyDrag(Rigidbody r)
    {
        r.drag = insideDrag;
        r.maxLinearVelocity = 0.25f;
        r.velocity = Vector3.zero;
    }

    private void UnapplyDrag(Rigidbody r)
    {
        r.drag = initialDrag;
        r.maxLinearVelocity = initialMaxLinearVelocity;
    }
}
