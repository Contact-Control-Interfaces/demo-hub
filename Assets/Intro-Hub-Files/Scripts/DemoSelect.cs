using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DemoSelect : MonoBehaviour
{
    [SerializeField] public Text demoTitle;
    [SerializeField] public Image demoImageHolder;


    [SerializeField] public DemoItem selected; //Test
    [SerializeField] public DemoItem oldSelect = null; //Test

    [Header("Desired Demo")]
    [SerializeField] public StartDemo sceneNum;

    public bool hasSelection;

    [SerializeField] private Color selectedColor;
    [Space(5)]
    [SerializeField] private Color unSelectedColor;

    public AudioSource clickAudio;

    public void newSelectedDemo(DemoItem demo)
    {
        if (oldSelect == null)
        {
            oldSelect = demo;
            hasSelection = true;
        }

        oldSelect.isSelected = false;
        oldSelect.GetComponent<Image>().color = unSelectedColor;

        selected = demo;
        selected.isSelected = true;
        sceneNum.desiredScene = selected.whichScene;
        selected.GetComponent<Image>().color = selectedColor;
        oldSelect = selected;
        clickAudio.Play();
        hasSelection = true;
    }
}
