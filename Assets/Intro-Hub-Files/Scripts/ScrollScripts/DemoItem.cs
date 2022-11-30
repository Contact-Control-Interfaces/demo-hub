using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DemoItem : MonoBehaviour
{

    [SerializeField]
    public int whichScene;

    public Text dNameText;

    [SerializeField]
    public string dName;

    [SerializeField]
    private DynamicScrollView dynamicScrollView;

    [SerializeField]
    public bool isSelected;

    public void Start()
    {
        dynamicScrollView = FindObjectOfType<DynamicScrollView>();
    }

    public void DemoSelected()
    {
        dynamicScrollView.newSelectedDemo(this);
        dNameText.text = dName;
        
    }

    public void GoToScene()
    {
        SceneManager.LoadScene(whichScene);
    }
}
