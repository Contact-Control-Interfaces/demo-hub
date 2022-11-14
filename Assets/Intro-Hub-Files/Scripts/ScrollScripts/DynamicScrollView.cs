using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicScrollView : MonoBehaviour
{
    //Any variables with Test commented after are to be removed prior to merge.

    [Header("Scroll Setup")]
    [SerializeField] private Transform scrollViewContent;
    [SerializeField] private DemoItem iconPrefab;

    [Header("Demo Setup")]
    [SerializeField] private List<DemoItem> demoIcons;
    [SerializeField] public Text demoTitle;
    [SerializeField] public DemoItem selected; //Test
    [SerializeField] public DemoItem oldSelect = null; //Test

    [Header("Desired Demo")]
    [SerializeField] public StartDemo sceneNum;



    [Header("Scroll Behavior")]
    [SerializeField] private Color selectedColor;
    [Space(5)]
    [SerializeField] private Color unSelectedColor;
    [Space(5)]
    [Range(0f, 1f)]
    [SerializeField] public float scrollSpeed = 0.01f;


    //Scrolling Logic
    [Header("Scroll Components")]
    [SerializeField] private ScrollRect Content;
    //[SerializeField] private ScrollButton upButton;
    //[SerializeField] private ScrollButton downButton;
    [SerializeField] private ScrollButton leftButton;
    [SerializeField] private ScrollButton rightButton;

    [SerializeField]
    public AudioSource clickAudio;



    private void Start()
    {


        for(int i = 0; i < demoIcons.Count; i++)
        {

            DemoItem newDemoButton = Instantiate(demoIcons[i], scrollViewContent);
            newDemoButton.dNameText = demoTitle;
        }
    }

    private void Update()
    {
      //Adds Up & Down Functionality
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
            clickAudio.Play();
        }
    }

    private void scrollLeft()
    {
        if (Content.horizontalNormalizedPosition >= 0f)
        {
            Content.horizontalNormalizedPosition -= scrollSpeed;
            clickAudio.Play();
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

        clickAudio.Play();
    }
}
