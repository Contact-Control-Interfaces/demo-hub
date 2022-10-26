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


    public void DemoSelected()
    {
        dNameText.text = dName;
    }
    
    public void GoToScene()
    {
        SceneManager.LoadScene(whichScene);
    }
}
