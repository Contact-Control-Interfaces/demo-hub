using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public GameObject SwitchSpawnedObject;
    public GameObject ButtonSpawnedObject;
    public Transform spawnPoint;
    public List<GameObject> pipeComponents;
    public AttachHandler attachHandler;

    public void SwitchSpawn()
    {
        GameObject switchSpawnedObject = Instantiate(SwitchSpawnedObject, spawnPoint);
        SwitchSpawnedObject.transform.localScale = new Vector3(1, 1, 1);
        attachHandler.ListenToSpawner(switchSpawnedObject.GetComponent<MaestroGrabbable>());

        foreach (GameObject go in pipeComponents)
        {
            Physics.IgnoreCollision(switchSpawnedObject.GetComponent<Collider>(), go.GetComponent<Collider>(), true);
        }
    }

    public void ButtonSpawn()
    {
        GameObject spawnedObject = Instantiate(ButtonSpawnedObject, spawnPoint);
        spawnedObject.transform.localScale = new Vector3(.25f, .25f, .25f);

        foreach (GameObject go in pipeComponents)
        {
            Physics.IgnoreCollision(spawnedObject.GetComponent<Collider>(), go.GetComponent<Collider>(), true);
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.S)) { SwitchSpawn(); }
        if (Input.GetKeyDown(KeyCode.B)) { ButtonSpawn(); }

    }
}
