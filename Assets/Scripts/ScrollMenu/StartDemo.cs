using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartDemo : MonoBehaviour
{

    public int desiredScene; //Test

    public AudioSource startAudio;

    public DynamicScrollView dsv;
    public DemoSelect ds;

    public void Start()
    {

    }

    public void StartScene()
    {
        if (dsv.hasSelection || ds.hasSelection)
        {
            startAudio.Play();
            SceneManager.LoadScene(desiredScene);
        }
        
    }


}
