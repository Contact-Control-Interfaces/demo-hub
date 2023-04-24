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

    public Image menuImageHolder;
    public Image demoImage;

    //[SerializeField]
    //private DynamicScrollView dynamicScrollView;

    [SerializeField]
    private DemoSelect demoSelector;

    [SerializeField]
    public bool isSelected;

    public BoxCollider demoCollision;

    public void Start()
    {
        //dynamicScrollView = FindObjectOfType<DynamicScrollView>();
    }   

    public void DemoSelected()
    {
        //dynamicScrollView.newSelectedDemo(this);
        demoSelector.newSelectedDemo(this);
        dNameText.text = dName;

        if (menuImageHolder != null) {
            menuImageHolder.sprite = demoImage.sprite;
        }
    }

    public void GoToScene()
    {
        SceneManager.LoadScene(whichScene);
    }

    public void DisableCollision()
    {
        demoCollision.enabled = false;
    }

    public void EnableCollision()
    {
        demoCollision.enabled = true;
    }
}
