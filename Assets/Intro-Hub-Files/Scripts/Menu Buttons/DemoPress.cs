using Leap.Unity;
using Maestro;
using Maestro.Vibration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoPress : MonoBehaviour
{

    [Header("Set Time")]
    public float baseTime;
    private float remainingTime;

    [Header("Properties")]

    public int assignedScene;
    public TextMeshProUGUI holdText;


    private bool timeOn;
    protected int CurrentlyColliding = 0;
    protected Coroutine CurrentCoroutine = null;
    private MaestroInteractable interactable;

    //value for the fill shader to not fill the button
    private float emptyNum = -.002f;
    private float fullNum = .002f;

    //value being manipulated and filling the button
    private float fillNum = 0;

    public Material buttonMaterial;

    void Start()
    {
        buttonMaterial.SetFloat("FillRate", emptyNum);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            StartTime();
        }
    }


    public void Register(FingerCollider fc)
    {
        CurrentlyColliding++;
        TryStartTime();
    }

    public void ButtonRegister()
    {
        CurrentlyColliding++;
        TryStartTime();
    }

    public void ButtonDeRegister()
    {
        interactable.ResetOverride();
        CurrentlyColliding--;

        if (CurrentlyColliding <= 0)
        {
            StopTime();
        }

    }

    public void Deregister(FingerCollider fc)
    {
        interactable.ResetOverride();
        CurrentlyColliding--;

        if (CurrentlyColliding <= 0)
        {
            StopTime();
        }
    }

    public void TryStartTime()
    {
        if (CurrentCoroutine == null && CurrentlyColliding > 0)
        {
            StartTime();
        }
    }

    void OnDisable()
    {
        if (CurrentCoroutine != null)
        {
            StopCoroutine(CurrentCoroutine);
            CurrentCoroutine = null;
        }
    }

    public void StartTime()
    {
        fillNum = emptyNum;
        timeOn = true;
        remainingTime = baseTime;
        if (CurrentCoroutine != null)
        {
            StopCoroutine(CurrentCoroutine);
            CurrentCoroutine = null;
        }

        if (this.gameObject.activeInHierarchy)
            CurrentCoroutine = StartCoroutine(UpdateTimer());  
    }

    public void StopTime()
    {
        fillNum = emptyNum;
        buttonMaterial.SetFloat("FillRate", emptyNum);
        timeOn = false;
        if (CurrentCoroutine != null)
        {
            StopCoroutine(CurrentCoroutine);
            CurrentCoroutine = null;
            holdText.gameObject.SetActive(false);
        }

        ResetDisplay();
    }

    private void ShaderValueUp()
    {
        fillNum += (fullNum - emptyNum) / 300f;
        buttonMaterial.SetFloat("FillRate", fillNum);

    }

    private IEnumerator UpdateTimer()
    {
        while (remainingTime > 0)
        {
            if (!timeOn)
                break;           

            remainingTime--;
            ShaderValueUp();

            holdText.gameObject.SetActive(true);

            yield return new WaitForSeconds(.01f);
        }
        SceneManager.LoadScene(assignedScene);
    }

    public void OnEnd()
    {
        buttonMaterial.SetFloat("FillRate", emptyNum);
        SceneManager.LoadScene(assignedScene);
        holdText.gameObject.SetActive(true);
        holdText.text = "Done";
    }

    private void ResetDisplay()
    {
        fillNum = emptyNum;
        remainingTime = baseTime;
    }
}
