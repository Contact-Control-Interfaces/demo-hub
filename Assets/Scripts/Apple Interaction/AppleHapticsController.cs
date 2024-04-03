using Maestro.Vibration;
using Maestro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleHapticsController : MonoBehaviour
{
    [Header("Bite timing")]
    public float firstBiteDuration = 0.15f;
    public float secondBiteDuration = 0.8f;
    public float eachBiteDuration = 0.1f;

    public float BiteDuration { get => secondBiteDuration + eachBiteDuration; }

    [Header("Haptic Effects")]
    public HapticEffect DropEffect = new HapticEffect() { Amplitude = 255, Vibration = new SoftBump(WideThreeOptions._100) { OneShot = true } };
    public float DropEffectDuration = 500; //ms

    public bool isDropping { get; private set; }
    public bool isBiting { get; private set; }
    public IEnumerator BiteHaptics()
    {
        isBiting = true;
        MaestroInteractable interactable = GetComponent<MaestroInteractable>();

        interactable.SendHapticsToWholeHand = true;

        yield return new WaitForSeconds(firstBiteDuration);
        interactable.stayHaptics.Vibration = new SharpTick(NarrowThreeOptions._100);
        interactable.stayHaptics.Vibration.OneShot = true;
        yield return new WaitForSeconds(eachBiteDuration);
        interactable.stayHaptics.Vibration = VibrationEffect.None;

        yield return new WaitForSeconds(secondBiteDuration - (firstBiteDuration + eachBiteDuration));
        interactable.stayHaptics.Vibration = new DoubleSharpTick(TickDuration.Short, NarrowThreeOptions._100);
        interactable.stayHaptics.Vibration.OneShot = true;
        yield return new WaitForSeconds(eachBiteDuration);
        interactable.stayHaptics.Vibration = VibrationEffect.None;

        interactable.SendHapticsToWholeHand = false;
        isBiting = false;
    }

    public IEnumerator DropHaptics()
    {
        isDropping = true;
        var interactable = GetComponent<MaestroInteractable>();
        interactable.SendHapticsToWholeHand = true;
        interactable.SetHapticOverride(DropEffect);
        yield return new WaitForSeconds(DropEffectDuration / 1000f);

        interactable.ResetOverride();
        interactable.SendHapticsToWholeHand = false;
        isDropping = false;
    }

    private void OnDisable()
    {
        if (isDropping)
        {
            StopCoroutine(DropHaptics());
        }

        if(isBiting)
        {
            StopCoroutine(BiteHaptics());
        }
    }
}
