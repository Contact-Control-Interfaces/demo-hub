using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ToggleEvent : UnityEvent<bool> { }

public class ToggleSwitchBehavior : MonoBehaviour
{
    public bool Toggled;
    private bool lastToggled = false;

    public ConfigurableJoint cj;

    public ToggleEvent onToggleChanged;
    public UnityEvent onToggleOn, onToggleOff;

    private AudioSource source;
    private float upPitch = 2.0f;
    private float downPitch = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        UpdateToggled();

        if (onToggleOn == null)
            onToggleOn = new UnityEvent();

        if (onToggleOff == null)
            onToggleOff = new UnityEvent();

        source = GetComponent<AudioSource>();
        if (source == null)
            source = this.gameObject.AddComponent<AudioSource>();

        source.volume = 0.5f;
        source.playOnAwake = false;
        source.clip = Resources.Load<AudioClip>("Sounds/switch");
    }

    // Update is called once per frame
    void Update()
    {
        UpdateToggled();
    }

    private void UpdateToggled()
    {
        float z = this.transform.localRotation.eulerAngles.z;
        Toggled = !(z <= 28);
        if (Toggled && !lastToggled)
        {
            if (!source.isPlaying)
            {
                source.pitch = upPitch;
                source.Play();
            }

            onToggleOn.Invoke();
            if (onToggleChanged != null)
                onToggleChanged.Invoke(true);
        } else if (!Toggled && lastToggled)
        {
            if (!source.isPlaying)
            {
                source.pitch = downPitch;
                source.Play();
            }

            onToggleOff.Invoke();
            if (onToggleChanged != null)
                onToggleChanged.Invoke(false);
        }

        cj.targetRotation = Quaternion.Euler(0, 0, 27 * (Toggled ? 1 : -1));

        lastToggled = Toggled;
    }
}
