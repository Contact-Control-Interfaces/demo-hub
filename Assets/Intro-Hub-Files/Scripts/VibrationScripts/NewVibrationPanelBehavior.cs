using System.Collections;
using System.Collections.Generic;
using System.IO;
using Maestro;
using Maestro.Vibration;
using UnityEditor;
using UnityEngine;

namespace Maestro
{
public class NewVibrationPanelBehavior : MonoBehaviour
{
  

    public List<MaestroInteractable> Interactables;
    
    // Start is called before the first frame update
    void Start()
    {
        var rad = GetComponentInChildren<RadioButtonPanel>();
        if (rad != null)
        {
            for (int i = 0; i < rad.gangedButtons.Count; i++)
            {
                var bt = rad.gangedButtons[i];
                var ren = bt.GetComponentInChildren<MeshRenderer>();
                if (ren == null)
                    continue;
                    if (Application.isPlaying)
                        Debug.Log("Start");
                    Interactables[1].enabled= true;
                    //ren.material.color = paletteColors[i];
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchOrbs (int index)
    {
        for(int i = 0; i < Interactables.Count; i++)
            {
                if(i == index)
                {
                    Interactables[i].enabled = true;
                }
                else 
                {
                    Interactables[i].enabled = false;
                }
            }
    }
}
}