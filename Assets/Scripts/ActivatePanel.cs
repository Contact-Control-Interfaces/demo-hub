using Maestro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ActivatePanel : MonoBehaviour
{
    public List<GameObject> panelButtons= new List<GameObject>();
    public List<bool> active = new List<bool>();
    public ConveyorBelt conveyorBelt;

    public UnityEvent OnOff, OnAllOn;

    public void Start()
    {
        ToggleCheck();
        OnOff.Invoke();
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

        if (active.All(currentBool => currentBool == true))
        {
            OnAllSwitchesToggled();
            OnAllOn.Invoke();
        }
        else
        {
            OnOff.Invoke();
        }
    }

    public void NetralHit()
    {
        Debug.Log("Neutral Hit");
    }

    public void OnAllSwitchesToggled()
    {
        conveyorBelt.ActivateBelt();
        SpawnPoint spawnPoint = FindAnyObjectByType<SpawnPoint>();
        spawnPoint.SwitchSpawn();

    }
}
