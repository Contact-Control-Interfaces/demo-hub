using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollisionsWith : MonoBehaviour
{
    public Collider[] toIgnore;

    public void Awake()
    {
        var ownColliders = this.GetComponents<Collider>();
        foreach (Collider a in ownColliders) {
            foreach (Collider b in toIgnore) {
                Physics.IgnoreCollision(a, b);
            }
        }
    }
}
