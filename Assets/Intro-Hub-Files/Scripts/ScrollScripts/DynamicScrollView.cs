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
    [SerializeField] public DemoItem selected;
    [SerializeField] public DemoItem oldSelect = null;


    [SerializeField] public StartDemo sceneNum;
    




    [SerializeField]
    private Color selectedColor;

    [SerializeField]
    private Color unSelectedColor;

    //Scrolling Logic

    [SerializeField]
    private ScrollRect Content;
    //[SerializeField] private ScrollButton upButton;
    //[SerializeField] private ScrollButton downButton;
    [SerializeField] private ScrollButton leftButton;
    [SerializeField] private ScrollButton rightButton;
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
        /*if(upButton.isDown)
        {
            scrollUp();
        }

        else if(downButton.isDown)
        {
            scrollDown();
        }*/
        if(rightButton.isDown)
        {
            scrollRight();
        }

        else if(leftButton.isDown)
        {
            scrollLeft();
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

    private void scrollRight()
    {
        if (Content.horizontalNormalizedPosition <= 1f)
        {
            Content.horizontalNormalizedPosition += scrollSpeed;
        }
    }

    private void scrollLeft()
    {
        if (Content.horizontalNormalizedPosition >= 0f)
        {
            Content.horizontalNormalizedPosition -= scrollSpeed;
        }
    }

    public void newSelectedDemo(DemoItem demo)
    {
        if(oldSelect == null)
        {
            oldSelect = demo;
        }

        oldSelect.isSelected = false;
        oldSelect.GetComponent<Image>().color = unSelectedColor;
        
        selected = demo;
        selected.isSelected = true;
        sceneNum.desiredScene = selected.whichScene;
        selected.GetComponent<Image>().color = selectedColor;
        oldSelect = selected;
    }





}
