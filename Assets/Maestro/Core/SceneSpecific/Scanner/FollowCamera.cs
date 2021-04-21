using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour {

    public Transform toFollow;
    public Vector3 offset;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 trueOffset = getOffset();

        this.transform.position = toFollow.position + Vector3.Scale(trueOffset, new Vector3(1, 0, 0)) + (this.transform.up * offset.y) + (this.transform.forward * offset.z);
        this.transform.rotation = Quaternion.LookRotation(Vector3.Scale(-trueOffset, new Vector3(1, 0, 1)));
	}

    private Vector3 getOffset()
    {
        return (toFollow.right.normalized * offset.x) + (this.transform.up * offset.y) + (toFollow.forward.normalized * offset.z);
    }
}
