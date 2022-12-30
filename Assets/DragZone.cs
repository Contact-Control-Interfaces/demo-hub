using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragZone : MonoBehaviour
{
    private static int InsideHowManyZones = 0;
    private float initialDrag;

    public float insideDrag;

    public Collider appleCollider;

    private void OnTriggerEnter(Collider other)
    {
        if (other == appleCollider) {
            Rigidbody r = other.GetComponent<Rigidbody>();
            if (r != null) {
                if (InsideHowManyZones == 0) {
                    initialDrag = r.drag;
                    ApplyDrag(r);
                }

                InsideHowManyZones++;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == appleCollider) {
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
    }

    private void UnapplyDrag(Rigidbody r)
    {
        r.drag = initialDrag;
    }
}
