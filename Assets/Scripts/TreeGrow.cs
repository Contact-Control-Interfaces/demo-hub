using Maestro.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class TreeGrow : MonoBehaviour
{
    public Animator treeAnimation;
    public Animator handAnimation;
    public GameObject appleDropTriggerArea;

    private int Animate;
    private int AnimateHand;

    void Awake()
    {
        Animate = Animator.StringToHash("Animate");
        AnimateHand = Animator.StringToHash("Animate Hand");
    }

    public void Start()
    {
        GrowTree();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            GrowTree();
        }

        if(OVRInput.Get(OVRInput.Button.Two))
        {
            GrowTree();
        }
    }

    public void GrowTree()
    {
        treeAnimation.SetBool(Animate, true);
        Invoke("StartHand", 5f);
    }
    
    private void StartHand()
    {
        handAnimation.SetBool(AnimateHand, true);
        appleDropTriggerArea.SetActive(true);
    }
}
