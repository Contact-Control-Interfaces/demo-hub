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
        GameObject spawnedObject = Instantiate(SpawnedObject, spawnPoint);

        foreach(GameObject go in pipeComponents)
        {
            Physics.IgnoreCollision(spawnedObject.GetComponent<Collider>(), go.GetComponent<Collider>(), true);
        }
        
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.S)) { Spawn(); }
    }
}
