using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuToggle : MonoBehaviour
{
    public Transform playerHead;
    public GameObject demoMenu;
    public float spawnDistance;

    // Start is called before the first frame update
    void Start()
    {
        demoMenu.SetActive(false);
    }

    private void Update()
    {
        demoMenu.transform.position = playerHead.position + new Vector3(playerHead.forward.x, 0, playerHead.forward.z).normalized * spawnDistance;
        demoMenu.transform.LookAt(playerHead.position);
        demoMenu.transform.forward *= -1;
    }


    public void ToggleObject()
    {   
        demoMenu.SetActive(!demoMenu.activeSelf);
    }

 
}
