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
using UnityEngine.UI;

public class DemoPress : MonoBehaviour
{
    [Header("Set Time")]
    public float pressLength;

    [Header("Properties")]

    public string SceneName;
    public TextMeshProUGUI holdText;
    public Sprite demoSprite;
    public Image demoImageHolder;
    public Material buttonMaterial;
    public GameObject pressButton;

    //value for the fill shader to not fill the button
    private float emptyNum = -.0012f;
    private float fullNum = .0012f;

    //value being manipulated and filling the button
    private float fillNum = 0;


    private float timePressed;
    float elapsedTime;

    private bool timeOn;
    protected int CurrentlyColliding = 0;
    protected Coroutine CurrentCoroutine = null;
    private MaestroInteractable interactable;


    void Start()
    {
        demoImageHolder.sprite = demoSprite;
        pressButton.GetComponent<Renderer>().material = buttonMaterial;
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
        timePressed = Time.time;

        fillNum = emptyNum;
        timeOn = true;
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
        elapsedTime = Time.time - timePressed;
        fillNum = Mathf.Lerp(emptyNum, fullNum, elapsedTime/pressLength);
        buttonMaterial.SetFloat("FillRate", fillNum);
    }

    private IEnumerator UpdateTimer()
    {
        do
        {
            ShaderValueUp();
            holdText.gameObject.SetActive(true);

            yield return new WaitForSeconds(.01f);
        }
        while (elapsedTime < pressLength);
       
        SceneManager.LoadScene(SceneName);
    }

    public void OnEnd()
    {
        buttonMaterial.SetFloat("FillRate", emptyNum);
        SceneManager.LoadScene(SceneName);
        holdText.gameObject.SetActive(true);
        holdText.text = "Done";
    }

    private void ResetDisplay()
    {
        fillNum = emptyNum;
    }
}
