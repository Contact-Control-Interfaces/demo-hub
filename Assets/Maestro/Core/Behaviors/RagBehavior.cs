using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagBehavior : MonoBehaviour {

    private Cloth cloth;

    private void Start()
    {
        cloth = this.GetComponentInChildren<Cloth>();

        CapsuleCollider[] cols = this.GetComponents<CapsuleCollider>();
        cloth.capsuleColliders = cols;

        this.transform.localScale = new Vector3(this.transform.localScale.x/2, this.transform.localScale.y, this.transform.localScale.z);
    }

    private void Update()
    {
        if (cloth.sphereColliders.Length < 2)
        {
            FingerCollider[] fcs = GameObject.FindObjectsOfType<FingerCollider>();
            List<ClothSphereColliderPair> spheres = new List<ClothSphereColliderPair>();

            foreach (FingerCollider fc in fcs)
                spheres.Add(new ClothSphereColliderPair(fc.GetComponentInChildren<SphereCollider>()));

            foreach (ClothSphereColliderPair fc in cloth.sphereColliders)
                spheres.Add(fc);

            cloth.sphereColliders = spheres.ToArray();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //NOTHING
    }

    private void OnCollisionExit(Collision collision)
    {
        //NOTHING
    }
}
