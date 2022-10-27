using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuTest : MonoBehaviour
{

    [SerializeField]
    public GameObject Content;

    public void Demo1Click()
    {
        SceneManager.LoadScene("Fake1");
       
    }

    public void Demo2Click()
    {
        SceneManager.LoadScene("Fake2");
    }

    public void BackToMain()
    {
        SceneManager.LoadScene("Test-Scene");
    }

}
