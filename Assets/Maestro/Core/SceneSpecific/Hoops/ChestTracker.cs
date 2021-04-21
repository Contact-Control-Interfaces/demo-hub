using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestTracker : MonoBehaviour
{
    public Transform head, leftHand, rightHand;
    public float verticalOffset, forwardOffset;

    // Update is called once per frame
    void Update()
    {
        this.transform.position = head.position - (Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized * forwardOffset) - (Vector3.up * verticalOffset);
        Vector3 here = this.transform.position;

        Vector3 midpoint = (rightHand.position + leftHand.position) / 2;
        midpoint.y = here.y;
        Vector3 handForward = (midpoint - here).normalized;
        Vector3 headForward = head.forward.normalized;

        Vector3 temp = Quaternion.LookRotation((headForward * 0.8f) + (handForward * 0.2f)).eulerAngles;


        this.transform.rotation = Quaternion.Euler(0, temp.y, 0);
    }
}
