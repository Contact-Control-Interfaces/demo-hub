using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneFade : MonoBehaviour
{
    public Animator animator;
    private string SceneToLoad;

    public void FadeToLevel(string SceneName)
    {
        SceneToLoad = SceneName;
        animator.SetTrigger("FadeOut");

        GrabMaterials tempGM = FindObjectOfType<GrabMaterials>();
        if (tempGM != null)
        {
            tempGM.CameraToggle();
        }
    }

    public void OnFadeComplete()
    {
        SceneManager.LoadScene(SceneToLoad);
    }
}
