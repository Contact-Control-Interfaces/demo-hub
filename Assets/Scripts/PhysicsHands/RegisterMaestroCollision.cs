using Maestro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegisterMaestroCollision : MonoBehaviour
{
    public List<Collider> colliders = new List<Collider>();
    private void Start()
    {
        if (colliders.Count == 0)
        {
            colliders.Add(GetComponent<Collider>());
        }
        Debug.Log("Tried to enter function");
        if (colliders.Count > 0)
        {
            foreach (Collider collider in colliders)
            {
                MaestroHand.collidingBodies.Add(collider.GetInstanceID());
            }
            Debug.Log("MaestroHand collidingBodies Count: " + MaestroHand.collidingBodies.Count);
        }
        else
        {
            Debug.LogError("You haven't assigned any colliders to the RegisterMaestroCollision, and there isn't a collider on the gameObject this is attached to. GameObject is: " + gameObject.name);
        }
    }
}
