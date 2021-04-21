using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnobBehavior : MonoBehaviour {

    static bool hasFirstBeenTouched = false;

    public bool shouldFail = false;
    private bool attached;
    public Rigidbody door;

	// Use this for initialization
	void Start () {
        attached = true;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void TryGrab()
    {
        if (!hasFirstBeenTouched)
        {
            hasFirstBeenTouched = true;
            this.shouldFail = true;
        }

        if (shouldFail)
        {
            if (!attached)
                return;
            else
            {
                Rigidbody temp = this.GetComponent<Rigidbody>();
                Destroy(this.GetComponent<ConfigurableJoint>());
                temp.useGravity = true;
                temp.drag = 0;
                attached = false;
            }
        }
        else
        {
            door.isKinematic = false;
        }
    }

    public void TryRelease()
    {
        if (!attached)
        {
            this.GetComponent<Rigidbody>().useGravity = true;
        }   
    }
}
