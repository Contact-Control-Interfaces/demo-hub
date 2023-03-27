using Oculus.Platform;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigHandler : MonoBehaviour
{
    public GameObject tree;
    public GameObject configPanel;

    // Start is called before the first frame update
    void Start()
    {

        if(tree != null && configPanel != null)
        ConfigSettings.AppleDemoStart(tree, configPanel);
        else
        Debug.Log("Objects Don't Exist in the Scene");
    }

    void Update()
    {
        if(ConfigSettings.configCompleted)
        {
            if (tree != null && configPanel != null)
                ConfigSettings.DemoCheck(tree);
            else
                Debug.Log("Not in Tree Scene");
        }
    }

    public void ButtonPress()
    {
        ConfigSettings.CloseConfig(configPanel);
    }

}
