using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartDemo : MonoBehaviour
{

    public int desiredScene; //Test


    public void StartScene()
    {
        SceneManager.LoadScene(desiredScene);
    }

}
