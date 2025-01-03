using Maestro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoArea : MonoBehaviour
{
    public GameObject demoArea;
    public MaestroManager playerRig;

    public void OnTriggerExit(Collider other)
    {
       if(other.gameObject.GetComponent<MaestroManager>())
        {

        }
    }

    public void outofBoundsAlert()
    {

    }

    public void Reset()
    {
        
    }
}
