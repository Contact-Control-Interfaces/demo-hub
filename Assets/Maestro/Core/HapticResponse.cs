using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HapticResponse : MonoBehaviour {

    public MaestroInteractable Parent;
    
	public void Start () {
        if (!Parent)
            this.Parent = this.GetComponentInParent<MaestroInteractable>();

        if (!Parent)
        {
            Debug.Log("No MaestroInteractable on this object or any parent! Disabling component.");
            enabled = false;
        }
	}

    public void FixedUpdate()
    {
        GetResponse(this.Parent);
    }

    protected abstract void GetResponse(MaestroInteractable parent);
}
