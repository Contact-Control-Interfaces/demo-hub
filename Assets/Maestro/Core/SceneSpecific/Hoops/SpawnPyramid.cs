using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPyramid : MonoBehaviour {

    public BasketballSpawner spawner;

    public float between = 0.5f;

	// Use this for initialization
	void Start () {
        int levels = 4;
        for (int i = levels; i > 0; i--)
        {
            SpawnSquare(i, (((levels - i) + 0.5f) * between) * 0.75f, (levels - i) * (between/2) * (this.transform.right + this.transform.forward));
        }
        //SpawnSquare(1, between / 2);
	}

    private void SpawnSquare(int n, float height, Vector3 offset)
    {
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                spawner.SpawnAt(this.transform.position + (i * this.transform.right * between) + (j * this.transform.forward * between) + (height * this.transform.up) + offset, Quaternion.identity, true);
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
