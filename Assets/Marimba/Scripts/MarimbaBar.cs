using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MarimbaBar : MonoBehaviour
{
    private AudioSource source;

    private static float maxImpulse = 2.5f;

    private bool hasBeenHit = false;
    private float min, max;

    void Start()
    {
        source = this.GetComponent<AudioSource>();

        min = float.MaxValue;
        max = float.MinValue;
    }

    private void OnCollisionEnter(Collision collision)
    {
        hasBeenHit = true;

        float impulse = collision.relativeVelocity.magnitude;

        Debug.Log(impulse);

        if (impulse < min) {
            min = impulse;
        }

        if (impulse > max) {
            max = impulse;
        }

        if (source.isPlaying) {
            source.Stop();
        }

        source.volume = Mathf.Min(1f, impulse / maxImpulse);

        source.Play();
    }

    private void OnApplicationQuit()
    {
        if (hasBeenHit) {
            Debug.Log($"{min} - {max}");
        } else {
            Debug.Log("Nothing to report");
        }
    }
}
