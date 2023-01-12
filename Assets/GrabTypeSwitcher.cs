using Maestro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrabTypeSwitcher : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)){
            Switch();
        }
    }

    public void Switch()
    {
        Debug.Log("SWITCHED");

        var manager = FindObjectsOfType<MaestroManager>().First();
        if (manager.grabType == GrabType.Arcade) {
            manager.grabType = GrabType.Physics;
        } else {
            manager.grabType = GrabType.Arcade;
        }
    }
}
