using Maestro;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AttachPoint : MonoBehaviour
{
    [SerializeField]
    private GameObject wristWatch;
    private GameObject realWristWatch;

    [SerializeField]
    private Material fakeWatchDissolve;
    [SerializeField]
    private Material realWatchDissolve;

    [SerializeField]
    private bool leftGlove;

    private float timePressed;
    float elapsedTime;
    public float pressLength;
    public float waitLength;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            timePressed = Time.time;
            StartCoroutine(DDissolveValue());

        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            timePressed = Time.time;
            StartCoroutine(DDissolveValue());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<ConveyorObject>() && collision.gameObject.GetComponent<ConveyorObject>().isWatch)
        {
            wristWatch.SetActive(true);
            StartCoroutine(DDissolveValue());
            StartCoroutine(WaitForASecond());
            StartCoroutine(UDissolveValue());
        }
    }
    private void UpDissolve()
    {
        elapsedTime = Time.time - timePressed;
        realWatchDissolve.SetFloat("dissolveAmount", 0);
        float lerpValue = Mathf.Lerp(0, 2, elapsedTime / pressLength);
        realWatchDissolve.SetFloat("dissolveAmount", lerpValue);
    }

    private void DownDissolve()
    {
        elapsedTime = Time.time - timePressed;
        fakeWatchDissolve.SetFloat("dissolveAmount", 2);
        float lerpValue = Mathf.Lerp(2, 0, elapsedTime / pressLength);
        fakeWatchDissolve.SetFloat("dissolveAmount", lerpValue);
    }

    private IEnumerator DDissolveValue()
    {
        do
        {
            DownDissolve();
            UpDissolve();
            yield return new WaitForSeconds(.01f);
        }
        while (elapsedTime < pressLength);
    }

    private IEnumerator WaitForASecond()
    {
        do
        {
            yield return new WaitForSeconds(.01f);
        }
        while (elapsedTime < waitLength);
    }

    private IEnumerator UDissolveValue()
    {
        do
        {
            UpDissolve();
            yield return new WaitForSeconds(.01f);
        }
        while (elapsedTime < pressLength);
    }
} 