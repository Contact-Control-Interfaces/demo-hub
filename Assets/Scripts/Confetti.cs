using Maestro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Confetti : MonoBehaviour
{

    [SerializeField]
    private AudioSource popAudio;
    [SerializeField]
    private ParticleSystem confetti;
    [SerializeField]
    private LatchingButtonBehavior latchingButtonBehavior;
    private bool isPressed;

    public void Start()
    {
        latchingButtonBehavior.onDown.AddListener(Pop);
    }

    public void Pop()
    {
        if (!isPressed)
        {
            popAudio.Play();
            confetti.Play();
        }

        isPressed = !isPressed;
    }
}
