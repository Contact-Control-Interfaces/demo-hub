using Maestro;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectFill : MonoBehaviour
{
    //value for the button fill shader
    private float emptyNum = -.0012f;
    private float fullNum = .0012f;

    //value being manipulated and filling the wrist watch face
    private float fillNum = 0;
    private float circleFillNum = 0;
    public Image progressBar;

    public GameObject closeButton;

    public Coroutine FillCoroutine;
    private float timePressed;
    float elapsedTime;
    protected int CurrentlyColliding = 0;

    CustomHapticSender hapticSender;

    [Header("Button Details")]
    public Material buttonMaterial;

    [Header("Set Time")]
    public float pressLength;
    public TextMeshPro holdText;

    public WristMenu menu;

    void Start()
    {
        hapticSender = FindAnyObjectByType<CustomHapticSender>();
    }

    public void Fill(bool menuActive)
    {
        if (FillCoroutine == null)
        {
            timePressed = Time.time;
            if (!menuActive)
            {
                FillCoroutine = StartCoroutine(GazeUpdateTimer());
            }
            else if (menuActive)
            {
                buttonMaterial = closeButton.GetComponent<MeshRenderer>().material;
                buttonMaterial.SetFloat("FillRate", emptyNum);
                FillCoroutine = StartCoroutine(UpdateTimer());
            }
        }
    }

    public void StopFill()
    {
        if (FillCoroutine != null)
        {
            FillReset();
            StopCoroutine(FillCoroutine);
            FillCoroutine = null;
            hapticSender.StopHaptics();
        }
    }

    private void ShaderValueUp()
    {
        elapsedTime = Time.time - timePressed;
        fillNum = Mathf.Lerp(emptyNum, fullNum, elapsedTime / pressLength);
        buttonMaterial.SetFloat("FillRate", fillNum);
    }

    private void CircleValueUp()
    {
        elapsedTime = Time.time - timePressed;
        circleFillNum = Mathf.Lerp(0, 1, elapsedTime / pressLength);
        progressBar.fillAmount = circleFillNum;
    }

    public void OnEnd()
    {
        circleFillNum = 0;
        FillReset();
        if (holdText != null)
            holdText.gameObject.SetActive(true);
        FillCoroutine = null;
        menu.MenuOff();
    }

    private IEnumerator UpdateTimer()
    {
        do
        {
            ShaderValueUp();
            yield return new WaitForSeconds(.01f);
            if (holdText != null)
                holdText.gameObject.SetActive(true);
        }
        while (elapsedTime < pressLength);
        OnEnd();
    }

    private IEnumerator GazeUpdateTimer()
    {
        hapticSender.StartHaptics();
        do
        {
            CircleValueUp();
            yield return new WaitForSeconds(.01f);
        }
        while (elapsedTime < pressLength);
        FillCoroutine = null;
        menu.MenuOn();
        hapticSender.StopHaptics();
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
        CurrentlyColliding--;
        if (CurrentlyColliding <= 0)
        {
            StopFill();
        }
    }

    public void TryStartTime()
    {
        if (CurrentlyColliding > 0)
        {
            Fill(true);
        }
    }

    public void FillReset()
    {
        progressBar.fillAmount = 0;
        buttonMaterial.SetFloat("FillRate", emptyNum);
    }
}
