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

    public WristGaze wristGazeInfo;

    [SerializeField]
    private Material  heldObjectDissolve;
    [SerializeField]
    private Material wristObjectDissolve;

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
            StartCoroutine(DownDissolveValue());

        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            timePressed = Time.time;
            StartCoroutine(DownDissolveValue());
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.GetComponent<ConveyorObject>() && collision.gameObject.GetComponent<ConveyorObject>().isWatch)
        {
            wristWatch.SetActive(true);
            wristGazeInfo.wristMenu = wristWatch.gameObject.GetComponent<WristMenu>();
            wristGazeInfo.wristMenuObj = wristWatch.gameObject;
            wristGazeInfo.fillHandler = wristWatch.gameObject.GetComponent<ObjectFill>();

            Destroy(collision.gameObject);

            //StartCoroutine(DownDissolveValue());
            //StartCoroutine(UDissolveValue());
        }
    }
    private void UpDissolve()
    {
        elapsedTime = Time.time - timePressed;
        wristObjectDissolve.SetFloat("dissolveAmount", 0);
        float lerpValue = Mathf.Lerp(0, 2, elapsedTime / pressLength);
        wristObjectDissolve.SetFloat("dissolveAmount", lerpValue);
    }

    private void DownDissolve()
    {
        elapsedTime = Time.time - timePressed;
        wristObjectDissolve.SetFloat("dissolveAmount", 2);
        float lerpValue = Mathf.Lerp(2, 0, elapsedTime / pressLength);
        wristObjectDissolve.SetFloat("dissolveAmount", lerpValue);
    }

    private IEnumerator DownDissolveValue()
    {
        do
        {
            //DownDissolve();
            //UpDissolve();
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