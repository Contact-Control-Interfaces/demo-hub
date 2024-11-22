using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{

    public GameObject SpawnedObject;
    public Transform spawnPoint;
    public List<GameObject> pipeComponents;
    public AttachHandler attachHandler;

    public void Spawn()
    {
        GameObject spawnedObject = Instantiate(SpawnedObject, spawnPoint);
        attachHandler.ListenToSpawner(spawnedObject.GetComponent<MaestroGrabbable>());

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
