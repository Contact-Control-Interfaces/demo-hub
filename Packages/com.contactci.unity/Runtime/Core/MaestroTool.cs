using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Maestro;
using UnityEngine;

public class MaestroTool : MonoBehaviour
{
    public Rigidbody rigidBody;

    public MaestroHand parentHand;

    [HideInInspector]
    public MaestroInteractable touching;
    [HideInInspector]
    public MaestroInteractable lastTouching;

    [Category("Finger overrides")]
    [Tooltip("Send haptics to selected fingers")]
    public bool sendThumb = true;
    
    public bool sendIndex = true;

    public bool sendMiddle = true;

    public bool sendRing = true;

    public bool sendLittle = true;

    // Start is called before the first frame update
    void Start()
    {
        if (!rigidBody)
            rigidBody = GetComponent<Rigidbody>();
        if (!parentHand)
            parentHand = GetComponentInParent<MaestroHand>();
        if (parentHand)
            PickUp(parentHand);
    }

    public void PickUp(MaestroHand hand)
    {
        parentHand = hand;
        parentHand.heldTool = this;
    }

    public void PutDown()
    {
        if (parentHand.heldTool == this)
            parentHand.heldTool = null;
        parentHand = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnCollisionEnter(Collision c)
    {
        var ci = c.gameObject.GetComponent<MaestroInteractable>();
        if (!ci)
            return;

        lastTouching = touching;
        touching = ci;
    }

    private void OnCollisionExit(Collision c)
    {
        var ci = c.gameObject.GetComponent<MaestroInteractable>();
        if (!ci)
            return;

        //TODO: come up with a way to persist this value so we can play the TouchEnd haptics
        //self-clearing property?
        lastTouching = ci;
        touching = null;
    }
}
