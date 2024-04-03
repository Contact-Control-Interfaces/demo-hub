using Maestro;
using Maestro.Vibration;
using UnityEngine;
using Image = UnityEngine.UI.Image;
using System.Linq;

public class AppleDrop : MonoBehaviour
{
    [Header("Drop Components")]
    bool missed;
    public Apple apple;
    public Transform dropOrigin;

    [Header("Drop Object Material")]
    public Material promptFlickerOn;
    public Material promptFlickerOff;

    //Counter
    [Header("Countdown Components")]
    public TextType timeOnText;
    public float baseTime; //test
    public float remainingTime;
    public bool timeOn; //test

    public bool OnEndCalled;

    //Progress Bar Logic
    public Image progressBar;
    public float startTime;
    float elapsedTime;
    private float emptyNum = 0f;
    private float fullNum = 1f;
    public float fillNum = 0;

    private string placeText = " Place your hand here";
    private string timerOnText = " Please hold still...";

    //Apple Animation
    public Animator AppleAnimator;
    private int AnimateApple;

    [Header("Haptics")]
    [Tooltip("Haptic effect when hand enters the trigger area")]
    public HapticEffect EnterHaptics = new HapticEffect{ Amplitude = 50, Vibration = new SoftBump(WideThreeOptions._30){OneShot = true} };
    
    protected int CurrentlyColliding = 0;

    public IMaestroHand leftHand, rightHand;
    private MaestroInteractable interactable;
    
    private void Start()
    {
        AnimateApple = Animator.StringToHash("Apple Grow");
        timeOnText = FindObjectOfType<TextType>();
        timeOnText.TextGen(placeText);
        interactable = GetComponent<MaestroInteractable>();
        interactable.SendHapticsToWholeHand = true;

        // Find hands if they are unset
        var hands = FindObjectsOfType<IMaestroHand>();

        if (leftHand == null) {
            leftHand = hands.First(x => x.whichHand == WhichHand.LeftHand);
        }

        if (rightHand == null) {
            rightHand = hands.First(x => x.whichHand == WhichHand.RightHand);
        }
    }

    public void Update()
    {
            if (remainingTime > 0 && timeOn)
            {
                remainingTime -= Time.deltaTime;
                UpdateFill();
            }

            if (remainingTime < 0 && !OnEndCalled)
            {
                progressBar.fillAmount = fullNum;
                OnEnd();
                OnEndCalled = true;
            }
    }

    public void Register(FingerCollider fc)
    {
        interactable.SetHapticOverride(EnterHaptics);
        CurrentlyColliding++;
        TryStartTime();
        AppleAnimator.SetBool(AnimateApple, true);
    }

    public void Deregister(FingerCollider fc)
    {
        interactable.ResetOverride();
        CurrentlyColliding--;

        if (CurrentlyColliding <= 0) {
            StopTime();
            AppleAnimator.SetBool(AnimateApple, false);
        }
    }

    public void TryStartTime()
    {
        if (!timeOn && CurrentlyColliding > 0) {
            StartTime();
        }
    }

    #region Material Logic

    public void StartTime()
    {
        if (!apple.StillAttachedToTree)
            return; // Only change material, start timer when apple is on the tree

        startTime = Time.time;
        OnEndCalled = false;
        fillNum = 0;
        
        apple.renderer.material = promptFlickerOn;
        
        timeOn = true;
        remainingTime = baseTime;
        timeOnText.TextGen(timerOnText, true);
        AppleAnimator.SetBool(AnimateApple, true);
    }

    public void StopTime()
    {
        interactable.ResetOverride();
        
        if (!apple.StillAttachedToTree)
            return; // Only change material, start timer when apple is on the tree

        progressBar.fillAmount = emptyNum;
        apple.renderer.material = promptFlickerOff;
        timeOn = false;
        timeOnText.TextGen(placeText, true);

        //timeOnText.BackText();
        ResetDisplay();
    }

    public void ResetAppleDrop()
    {
        CurrentlyColliding = 0;
    }

    #endregion


    #region Timer
    private void UpdateFill()
    {
        elapsedTime = Time.time - startTime;
        fillNum = Mathf.Lerp(emptyNum, fullNum, elapsedTime/baseTime);
        progressBar.fillAmount = fillNum;
    }

    private void OnEnd()
    {
        if (apple == null || !apple.StillAttachedToTree)
            return;

        apple.Pluck();
        interactable.ResetOverride();
        timeOn = false;
        AppleAnimator.SetBool(AnimateApple, false);

        Vector3 leftOffset = GetOffset(leftHand);
        Vector3 rightOffset = GetOffset(rightHand);

        Vector3 leftLateral = GetLateralOffset(leftOffset);
        Vector3 rightLateral = GetLateralOffset(rightOffset);

        Vector3 offset = leftOffset;
        Vector3 lateral = leftLateral;
        if (rightLateral.sqrMagnitude < leftLateral.sqrMagnitude)
        {
            offset = rightOffset;
            lateral = rightLateral;
        }

        var rb = apple.GetComponent<Rigidbody>();
        rb.velocity += lateral / GetFallDuration(offset);

    }

    private Vector3 GetOffset(IMaestroHand hand)
    {
        return hand.transforms.MiddleMiddle.position - apple.transform.position;
    }

    private Vector3 GetLateralOffset(Vector3 offset)
    {
        return Vector3.Scale(offset, new Vector3(1, 0, 1));
    }

    private float GetFallDuration(Vector3 offsetFromPalm)
    {
        float fallDistance = Mathf.Abs(offsetFromPalm.y);
        float a = Mathf.Abs(Physics.gravity.y);
        return Mathf.Sqrt(2 * a * fallDistance) / a;
    }

    private void ResetDisplay()
    {
        remainingTime = baseTime;
    }

    #endregion
}
