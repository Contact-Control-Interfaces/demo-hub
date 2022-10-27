using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicScrollView : MonoBehaviour
{
    [SerializeField] private Transform scrollViewContent;
    [SerializeField] private DemoItem prefab;
    [SerializeField] private List<DemoItem> Demos;
    [SerializeField] public Text demoTitle;

    //Scrolling Logic

    [SerializeField]
    private ScrollRect Content;
    [SerializeField] private ScrollButton upButton;
    [SerializeField] private ScrollButton downButton;
    public float scrollSpeed = 0.01f;


    private void Start()
    {
        for(int i = 0; i < Demos.Count; i++)
        {

            DemoItem newDemoButton = Instantiate(Demos[i], scrollViewContent);
            newDemoButton.dNameText = demoTitle;
        }
    }

    private void Update()
    {
        if(upButton.isDown)
        {
            scrollUp();
        }

        else if(downButton.isDown)
        {
            scrollDown();
        }
    }

    private void scrollUp()
    {
        if(Content.verticalNormalizedPosition <= 1f)
        {
            Content.verticalNormalizedPosition += scrollSpeed;
        }
    }

    private void scrollDown()
    {
        if (Content.verticalNormalizedPosition >= 0f)
        {
            Content.verticalNormalizedPosition -= scrollSpeed;
        }
    }





}
