using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicScrollView : MonoBehaviour
{
    [SerializeField]
    private Transform scrollViewContent;

    [SerializeField]
    private DemoItem prefab;

    [SerializeField]
    private List<DemoItem> Demos;

    [SerializeField]
    public Text demoTitle;


    private void Start()
    {
        for(int i = 0; i < Demos.Count; i++)
        {

            DemoItem newDemoButton = Instantiate(Demos[i], scrollViewContent);
            newDemoButton.dNameText = demoTitle;
        }
    }


}
