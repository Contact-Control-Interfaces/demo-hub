using Leap.Unity.PhysicalHands;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PhysicsHandSetupHelper : MonoBehaviour
{
    private void Awake()
    {
        Collider[] colliders = FindObjectsOfType<Collider>(true);
        IEnumerable noGrabObjects = colliders.Where(col => col.GetComponent<MaestroGrabbable>() == null).Select(col => col.gameObject);
        foreach(GameObject col in noGrabObjects)
        {
            IgnorePhysicalHands ignorePhysicalHands = col.AddComponent(typeof(IgnorePhysicalHands)) as IgnorePhysicalHands;
            ignorePhysicalHands.DisableAllGrabbing = true;
            ignorePhysicalHands.DisableAllHandCollisions = false;
        }
    }
}
