using Maestro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActivatePanel : MonoBehaviour
{

    public List<GameObject> panelButtons= new List<GameObject>();
    public List<bool> active = new List<bool>();
    public ConveyorBelt conveyorBelt;

    public void Start()
    {
        ToggleCheck();
    }

    public void ToggleCheck()
    {
        foreach(GameObject go in panelButtons)
        {
            if(go.GetComponent<ToggleSwitchBehavior>() != null)
            {
                ToggleSwitchBehavior currentToggle = go.GetComponent<ToggleSwitchBehavior>();
                int index = panelButtons.IndexOf(go);
                active[index] = (currentToggle.state == ToggleState.On) ? true: false;

            }

            else if(go.GetComponent<IndustrialSwitchBehavior>() != null)
            {
                IndustrialSwitchBehavior currentToggle = go.GetComponent<IndustrialSwitchBehavior>();
                int index = panelButtons.IndexOf(go);
                active[index] = (currentToggle.state == IndustrialToggleState.On) ? true : false;
            }
        }

        Debug.Log( "All on?: " + AllActiveCheckDebug());
    }

    public void OnAllSwitchesToggled()
    {
        conveyorBelt.ActivateBelt();
        SpawnPoint spawnPoint = FindAnyObjectByType<SpawnPoint>();
        spawnPoint.SwitchSpawn();

    }

    public bool AllActiveCheckDebug()
    {
        string resultString = "";

        foreach (bool booleanVal in active)
        {
            resultString = resultString + " " + booleanVal;
        }

        if (active.All(currentBool => currentBool == true))
        {
            Debug.Log(resultString);
            OnAllSwitchesToggled();
            return true;
        }
        else
        {
            Debug.Log(resultString);
            return false;
        }
            
    }
}
