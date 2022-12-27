using Maestro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollisionHandler : MonoBehaviour
{
    public bool turnOnCollision;
    public DemoItem curDemoItem;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<SetBox>())
        {
            curDemoItem = other.gameObject.GetComponent<SetBox>().attachedDemoItem;

            curDemoItem.GetComponent<MaestroInteractable>().enabled = turnOnCollision;
        }
    }
}
