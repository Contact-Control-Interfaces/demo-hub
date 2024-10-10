using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

public class Belt : MonoBehaviour
{

    public List<GameObject> touchingObjects = new List<GameObject>();

    public bool endPoint;
    public bool processedObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<ConveyorObject>())
        {
            touchingObjects.Add(collision.gameObject);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!endPoint && !processedObject)
        {
            foreach (GameObject obj in touchingObjects)
            {
                Debug.Log("Push");
                obj.GetComponent<Rigidbody>().AddForce(this.transform.right * 500f);
            }
        }

        else if(endPoint && !processedObject)
        {
            foreach (GameObject obj in touchingObjects)
            {
                Debug.Log("Push");
                obj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationY;
                obj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            }

        }

        processedObject = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        touchingObjects.Clear();
        processedObject = false;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
