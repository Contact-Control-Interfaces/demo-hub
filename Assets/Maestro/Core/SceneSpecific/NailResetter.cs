using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NailResetter : MonoBehaviour {

    NailBehavior[] nails;
    float initialHeight;
    //public float maxHeight;
    public float minOffset;
    public static GameObject smokePrefab;

    // Use this for initialization
    void Start()
    {
        if (!smokePrefab)
            smokePrefab = GameObject.Find("Smoke");

        this.nails = this.GetComponentsInChildren<NailBehavior>();
        initialHeight = nails[0].transform.position.y;
	}
	
	// Update is called once per frame
	void Update () {
        bool reset = true;
        foreach (NailBehavior nb in nails)
        {
            reset = reset && nb.transform.position.y <= initialHeight + minOffset;

            if (nb.transform.position.y > initialHeight)
            {
                Vector3 temp = nb.transform.position;
                temp.y = initialHeight;
                nb.transform.position = temp;
            }

            /*if (nb.transform.position.y >= maxHeight && !nb.detached)
            {
                nb.detached = true;

                Rigidbody rb = nb.GetComponent<Rigidbody>();
                rb.useGravity = true;
                rb.constraints = RigidbodyConstraints.None;

                CapsuleCollider cap = nb.GetComponent<CapsuleCollider>();
                cap.height = cap.height + 11f;
                cap.center = new Vector3(cap.center.x, cap.center.y + 5, cap.center.z);
            }*/
        }

        if (reset)
        {
            Debug.Log("Resetting nails");
            foreach (NailBehavior nb in nails)
            {
                Vector3 temp = nb.transform.position;
                temp.y = initialHeight;
                StartCoroutine("SpawnSmoke", nb.transform.position = temp);

                Rigidbody rb = nb.GetComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
                rb.useGravity = false;

                nb.transform.parent = this.transform;
            }
        }
	}

    private IEnumerator SpawnSmoke(Vector3 location)
    {
        if (smokePrefab)
        {
            GameObject smoke = Instantiate(smokePrefab);
            smoke.transform.position = location;
            smoke.transform.localScale = 0.05f * Vector3.one;
            yield return new WaitForSeconds(2.0f);
            Destroy(smoke);
        }
    }
}
