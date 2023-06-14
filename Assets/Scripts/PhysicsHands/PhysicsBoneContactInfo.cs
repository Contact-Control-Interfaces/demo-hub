using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsBoneContactInfo : MonoBehaviour
{
    public Collision fingerCollision;
    private void OnCollisionEnter(Collision collision)
    {
        fingerCollision = collision;
    }

    private void OnCollisionExit(Collision collision)
    {
        fingerCollision = null;
    }
}
