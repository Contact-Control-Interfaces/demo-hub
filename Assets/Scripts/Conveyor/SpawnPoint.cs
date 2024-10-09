using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{

    public GameObject SpawnedObject;
    public Transform spawnPoint;
    public List<GameObject> pipeComponents;

    public void Spawn()
    {

        GameObject currentCube = Instantiate(SpawnedObject, spawnPoint);

        foreach(GameObject go in pipeComponents)
        {
            Physics.IgnoreCollision(currentCube.GetComponent<Collider>(), go.GetComponent<Collider>(), true);
        }
        
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.S)) { Spawn(); }
    }
}
