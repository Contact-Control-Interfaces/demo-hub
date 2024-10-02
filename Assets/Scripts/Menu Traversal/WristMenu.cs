using JetBrains.Annotations;
using Maestro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WristMenu : MonoBehaviour
{
    public FollowGaze menu;
    public GrabMaterials materialGrabber;

    public UnityEvent onMenuActivate;
    public UnityEvent onMenuDeactivate;

    // Start is called before the first frame update
    void Start()
    {
        menu.transform.parent = null;
    }

    public void MenuOn()
    {
        if (materialGrabber != null)
        {
            materialGrabber?.ApplyGhostShader();
        }
        onMenuActivate?.Invoke();
    }

    public void MenuOff()
    {
        if (materialGrabber != null)
        {
            materialGrabber?.RemoveGhostShader();
        }
        onMenuDeactivate?.Invoke();
    }
}
