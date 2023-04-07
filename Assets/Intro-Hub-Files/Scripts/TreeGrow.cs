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


    private int Animate;
    private int AnimateHand;

    // Start is called before the first frame update
    void Start()
    {
        Animate = Animator.StringToHash("Animate");
        AnimateHand = Animator.StringToHash("Animate Hand");
    }

    // Update is called once per frame
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
        DisplayBLE.SetLeftText("Tree Growing");
        treeAnimation.SetBool(Animate, true);
        Invoke("StartHand", 5f);

    }
    
    private void StartHand()
    {
        handAnimation.SetBool(AnimateHand, true);
    }
}
