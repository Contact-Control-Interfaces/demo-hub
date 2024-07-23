using Maestro;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MenuToggle : MonoBehaviour
{
    [Header("Set Time")]
    public float pressLength;

    [Header("Menu Details")]
    public Transform playerHead;
    public FollowGaze demoMenu;
    public TextMeshPro holdText;

    public GrabMaterials materialGrabber;

    [Header("Button Details")]
    public Material buttonMaterial;
    public MeshRenderer buttonRender;
    public GameObject pressButton;
    public AudioSource menuAudio;

    //value for the fill shader to not fill the button
    private float emptyNum = -.0012f;
    private float fullNum = .0012f;

    //value being manipulated and filling the button
    private float fillNum = 0;

    protected Coroutine CurrentCoroutine = null;
    private float timePressed;
    float elapsedTime;

    protected int CurrentlyColliding = 0;

    private MaestroInteractable interactable;

    public UnityEvent onMenuActivate;
    public UnityEvent onMenuDeactivate;

    private void Start()
    {
        demoMenu.transform.parent = null; // detach from hand
    }

    public void ToggleObject()
    {
        menuAudio.Play();

        if (!demoMenu.Active)
            materialGrabber?.ApplyGhostShader();
        else
            materialGrabber?.RemoveGhostShader();

        if (!demoMenu.Active)
            onMenuActivate?.Invoke();
        else
            onMenuDeactivate?.Invoke();
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
        fillNum = Mathf.Lerp(emptyNum, fullNum, elapsedTime / pressLength);
        buttonMaterial.SetFloat("FillRate", fillNum);
    }

    private IEnumerator UpdateTimer()
    {
        do
        {
            ShaderValueUp();
            yield return new WaitForSeconds(.01f);
            holdText.gameObject.SetActive(true);
        }
        while (elapsedTime < pressLength);

        ToggleObject();
    }

    public void OnEnd()
    {
        buttonMaterial.SetFloat("FillRate", emptyNum);
        holdText.gameObject.SetActive(true);
        ToggleObject();
    }

    private void ResetDisplay()
    {
        fillNum = emptyNum;
    }
}
