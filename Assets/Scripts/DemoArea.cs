using Maestro;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DemoArea : MonoBehaviour
{
    public GameObject demoArea;
    public GameObject alertBox;
    public MaestroManager playerRig;


    float elapsedTime;
    public float waitLength;



public void OnTriggerExit(Collider other)
{
   if(other.gameObject.GetComponent<MaestroManager>())
    {

    }
}

public void outofBoundsAlert()
{

}

private IEnumerator AlertCountDown()
{
    do
    {
        yield return new WaitForSeconds(.01f);
    }
    while (elapsedTime < waitLength);
    resetLocation();
}

public void resetLocation()
{

}

