using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepTrackOfVelocity : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = this.transform.position;

        rb = this.GetComponent<Rigidbody>();
    }

    void Update()
    {
        rb.velocity = (this.transform.position - lastPosition) / Time.deltaTime;

        lastPosition = this.transform.position;
    }
}
