using Maestro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleDrop : MonoBehaviour
{

    //Materials
    public Material enteredMaterial;
    public Material inMaterial;
    public Material exitMaterial;
    public Material emptyMaterial;

    private bool hasHand = false;

    private void Update()
    {
        if(!hasHand)
        {
            this.GetComponent<Renderer>().material = emptyMaterial;
        }

        //this.GetComponent<Renderer>().material = (hasHand) ? inMaterial : emptyMaterial;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<FingerCollider>())
        {
            this.GetComponent<Renderer>().material = enteredMaterial;
            hasHand = true;

        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<FingerCollider>())
        {
            this.GetComponent<Renderer>().material = inMaterial;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        this.GetComponent<Renderer>().material = exitMaterial;
        hasHand = false;
    }
}
