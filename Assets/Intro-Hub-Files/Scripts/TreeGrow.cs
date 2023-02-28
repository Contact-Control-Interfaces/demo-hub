using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGrow : MonoBehaviour
{
    public Animator treeAnimation;


    private bool animate;

    // Start is called before the first frame update
    void Start()
    {   
        animate = treeAnimation.GetBool("Animate");
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
        treeAnimation.SetBool("Animate", true);
    }

    private void StopTree()
    {
        treeAnimation.SetBool("Animate", false);
    }


}
