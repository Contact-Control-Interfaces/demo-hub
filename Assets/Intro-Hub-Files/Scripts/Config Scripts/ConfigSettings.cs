using Maestro;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using static GloveSelect;

public static class ConfigSettings
{

    public static bool treeActive;
    public static bool configCompleted = false;
    public static DemoMode demoMode;

    public static void AppleDemoStart(GameObject tree, GameObject configPanel)
    {
        treeActive = false;
        
        if (configCompleted)
        {
            configPanel.SetActive(false);
            ActivateTree(tree);
        }
        else
        {
            configPanel.SetActive(true);
        }
    }

    public static void CloseConfig(GameObject configPanel)
    {
        configPanel.SetActive(false);
        configCompleted = true;
    }

    public static void ActivateTree(GameObject tree)
    {
        if (treeActive)
        {
            Debug.Log("Tree Active");
            return;
        }
        else
        {
            tree.SetActive(true);
            tree.GetComponent<TreeGrow>().GrowTree();
            treeActive = true;
        }
    }

    //Checks the Demo Mode: 0 = One Glove Demo | 1 = Two Glove Demo
    public static void DemoCheck(GameObject tree)
    {
        switch (demoMode)
        {
            case DemoMode.OneGlove:
                if (BluetoothKeepAlive.is_left_connected() || BluetoothKeepAlive.is_right_connected())
                {
                    ActivateTree(tree);
                }
                break;

            case DemoMode.TwoGlove:
                if (BluetoothKeepAlive.is_left_connected() && BluetoothKeepAlive.is_right_connected())
                {
                    ActivateTree(tree);
                }
                break;
        }
    }
}
