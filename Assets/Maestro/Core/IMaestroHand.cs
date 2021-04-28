using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum WhichHand
{
    RightHand, LeftHand
};

public enum InteractionPriority
{
    PrioritizeAmplitude, PrioritizeVibrationEffect /*, Cascade, Importance*/
};

public struct MaestroHapticContext
{
    public byte? ThumbAmplitude { get; set; }
    public byte? IndexAmplitude { get; set; }
    public byte? MiddleAmplitude { get; set; }
    public byte? RingAmplitude { get; set; }
    public byte? LittleAmplitude { get; set; }

    public byte? ThumbVibrationEffect { get; set; }
    public byte? IndexVibrationEffect { get; set; }
    public byte? MiddleVibrationEffect { get; set; }
    public byte? RingVibrationEffect { get; set; }
    public byte? LittleVibrationEffect { get; set; }

    public void SetAllAmplitudes(byte? amplitude)
    {
        ThumbAmplitude = amplitude;
        IndexAmplitude = amplitude;
        MiddleAmplitude = amplitude;
        RingAmplitude = amplitude;
        LittleAmplitude = amplitude;
    }

    public void SetAllVibrationEffects(byte? vibrationEffect)
    {
        ThumbVibrationEffect = vibrationEffect;
        IndexVibrationEffect = vibrationEffect;
        MiddleVibrationEffect = vibrationEffect;
        RingVibrationEffect = vibrationEffect;
        LittleVibrationEffect = vibrationEffect;
    }

    public void SetAmplitudeFromIndex(int index, byte? amplitude)
    {
        switch (index) {
            default:
            case 0: ThumbAmplitude = amplitude; break;
            case 1: IndexAmplitude = amplitude; break;
            case 2: MiddleAmplitude = amplitude; break;
            case 3: RingAmplitude = amplitude; break;
            case 4: LittleAmplitude = amplitude; break;
        }
    }

    public void SetVibrationEffectFromIndex(int index, byte? vibrationEffect)
    {
        switch (index) {
            default:
            case 0: ThumbVibrationEffect = vibrationEffect; break;
            case 1: IndexVibrationEffect = vibrationEffect; break;
            case 2: MiddleVibrationEffect = vibrationEffect; break;
            case 3: RingVibrationEffect = vibrationEffect; break;
            case 4: LittleVibrationEffect = vibrationEffect; break;
        }
    }
}

public abstract class IMaestroHand : MonoBehaviour
{
    public InteractionPriority interactionPriority {
        get;
        protected set;
    }

    public MaestroHapticContext lastHaptics {
        get;
        protected set;
    }
}