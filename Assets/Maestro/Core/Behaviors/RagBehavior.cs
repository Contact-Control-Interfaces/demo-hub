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
            FingerTipCollider[] ftcs = GameObject.FindObjectsOfType<FingerTipCollider>();
            List<ClothSphereColliderPair> spheres = new List<ClothSphereColliderPair>();

            Debug.Log(ftcs.Length + "asdasdadad");

            foreach (FingerTipCollider ftc in ftcs)
                spheres.Add(new ClothSphereColliderPair(ftc.GetComponentInChildren<SphereCollider>()));

            foreach (ClothSphereColliderPair ftc in cloth.sphereColliders)
                spheres.Add(ftc);

            cloth.sphereColliders = spheres.ToArray();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {

        //Debug.Log("Touched rag!!!!!!");

        FingerTipCollider ftc = collision.gameObject.GetComponent<FingerTipCollider>();

        if (ftc != null && ftc.isTip)
        {
            ftc.pocb.PaintAmp = 200;
            ftc.PaintColor = Color.clear;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        FingerTipCollider ftc = collision.gameObject.GetComponent<FingerTipCollider>();

        if (ftc != null && ftc.isTip)
        {
            ftc.pocb.PaintAmp = null;
        }
    }
}
