using Autodesk.Fbx;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    // Start is called before the first frame update
    
    public List<Belt> beltList = new List<Belt>();

    void Start()
    {
        foreach(Belt belt in beltList)
        {
            belt.enabled = false;
        }
    }

    public void ActivateBelt()
    {
        foreach(Belt belt in beltList)
        {
            belt.enabled = true;
        }

        beltList.Last<Belt>().endPoint = true;
    }
}
