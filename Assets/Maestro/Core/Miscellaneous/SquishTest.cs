using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquishTest : MonoBehaviour {

    public int size;
    public float radius = 0.2f;
    public float colliderRadius = 0.008f;
    private FingerTipCollider[] shell;
    private Vector3[] shellSpawn;
    
    public bool reset;
    private bool lastReset;

	// Use this for initialization
	void Start () {
        shell = new FingerTipCollider[size];
        shellSpawn = new Vector3[size];
        if (size > 0)
            SpawnColliders();
	}

    private void SpawnColliders()
    {
        float offset = (Mathf.PI * 2) / size;

        for (int i = 0; i < shell.Length; i++)
        {
            //making a new fingertip collider gameObject and naming it
            GameObject g = new GameObject();
            g.transform.parent = this.transform;
            Vector3 spawnLocation = new Vector3(0, radius * Mathf.Sin(offset * i), radius * Mathf.Cos(offset * i));
            shellSpawn[i] = spawnLocation;
            g.transform.localPosition = spawnLocation;
            g.AddComponent<Rigidbody>();
            shell[i] = g.AddComponent<FingerTipCollider>();
            shell[i].index = i;
            shell[i].hpi = null;
            shell[i].rb = shell[i].GetComponent<Rigidbody>();
            shell[i].makeRend(colliderRadius);
            shell[i].rb.mass = 0.1f;
            shell[i].rb.useGravity = false;
            shell[i].rb.freezeRotation = true;
        }
    }

    public void FixedUpdate()
    {
        for (int i = 0; i < shell.Length; i++)
        {
            shell[i].rb.velocity = (this.transform.position + shellSpawn[i] - shell[i].rb.position) / Time.deltaTime;
            //shell[i].rb.MoveRotation(shellSpawn[i].transform.rotation);

            shell[i].rend.enabled = true; //showColliders;

            //if the fingertip is touching something, and I'm not currently holding anything, check if there's more than one finger holding onto it.
            /*if (shell[i].touching != null)
            {
                if (!grabbing)
                {
                    checkFingerGrabbing(i);
                }
            }*/
        }

        Vector3 totalImpulse = Vector3.zero;
        for (int i = 0; i < shell.Length; i++)
        {
            totalImpulse += shell[i].NetImpulse;
        }

        this.GetComponent<Rigidbody>().AddForce(totalImpulse * 1000, ForceMode.Impulse);
    }
}
