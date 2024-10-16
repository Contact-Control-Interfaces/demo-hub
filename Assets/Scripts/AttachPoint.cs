using Maestro;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AttachPoint : MonoBehaviour
{
    [SerializeField]
    private GameObject wristWatch;

    [SerializeField]
    private Material dissolveEffect;

    private float timePressed;
    float elapsedTime;
    public float pressLength;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            timePressed = Time.time;
            StartCoroutine(UDissolveValue());
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            timePressed = Time.time;
            StartCoroutine(DDissolveValue());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<ConveyorObject>())
        {
            wristWatch.SetActive(true);
            StartCoroutine(DDissolveValue());
        }
    }
    private void UpDissolve()
    {
        elapsedTime = Time.time - timePressed;
        float lerpValue = Mathf.Lerp(0, 2, elapsedTime / pressLength);
        dissolveEffect.SetFloat("dissolveAmount", lerpValue);
    }

    private void DownDissolve()
    {
        elapsedTime = Time.time - timePressed;
        float lerpValue = Mathf.Lerp(2, 0, elapsedTime / pressLength);
        dissolveEffect.SetFloat("dissolveAmount", lerpValue);
    }

    private IEnumerator DDissolveValue()
    {
        do
        {
            UpDissolve();
            yield return new WaitForSeconds(.01f);
        }
        while (elapsedTime < pressLength);
    }

    private IEnumerator UDissolveValue()
    {
        do
        {
            DownDissolve();
            yield return new WaitForSeconds(.01f);
        }
        while (elapsedTime < pressLength);
    }
} 