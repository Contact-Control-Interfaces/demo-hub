using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AudioClipSettings
{
    public AudioClip AudioClip;
    public bool Loop;
    [Range(0,256)]
    public int Priority = 128;
    [Range(0f,1f)]
    public float Volume = 1f;
    [Range(-3f, 3f)]
    public float Pitch = 1;
    [Range(-1f,1f)]
    public float StereoPan;
    [Range(0f,1f)]
    public float SpatialBlend;
    [Range(0f,1.1f)]
    public float ReverbZoneMix;
}
public static class AudioUtilities
{
    public static void PlayAudioClipWithCustomSettings(this AudioSource audioSource, AudioClipSettings settings)
    {
        audioSource.loop = settings.Loop;
        audioSource.volume = settings.Volume;
        audioSource.pitch = settings.Pitch;
        audioSource.priority = settings.Priority;
        audioSource.panStereo = settings.StereoPan;
        audioSource.spatialBlend = settings.SpatialBlend;
        audioSource.reverbZoneMix = settings.ReverbZoneMix;
        audioSource.clip = settings.AudioClip;
        audioSource.Play();
    }
}
