using Maestro;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AttachPoint : MonoBehaviour
{
    [SerializeField]
    private GameObject wristWatch;
    public WristMenu wristMenu;
    public ObjectFill objectFill;
    private GameObject realWristWatch;

    public WristGaze wristGazeInfo;

    [SerializeField]
    private bool leftGlove;

/*    private float timePressed;
    float elapsedTime;
    public float pressLength;
    public float waitLength;*/

    /*
    [SerializeField]
    private Material  heldObjectDissolve;
    [SerializeField]
    private Material wristObjectDissolve;
    */


    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.GetComponent<ConveyorObject>() && collision.gameObject.GetComponent<ConveyorObject>().isWatch)
        {
           EnableWrist(leftGlove);
           FindAnyObjectByType<AttachHandler>().leftWatchOn = leftGlove;
           Destroy(collision.gameObject);
        }
    }
    
    public void EnableWrist(bool isTheLeftHandAttaching)
    {
        FindAnyObjectByType<WristWatchSwapUI>().LightUpGraphic(!isTheLeftHandAttaching);
        wristWatch.SetActive(true);
        wristGazeInfo.wristMenuObj = wristWatch.gameObject;
        wristGazeInfo.wristMenu = wristWatch.GetComponent<WristMenu>();
        wristGazeInfo.fillHandler = wristWatch.GetComponent<ObjectFill>();

        this.gameObject.SetActive(false);
    }

    public void DisableWrist()
    {
        wristWatch.SetActive(false);
    }

    /*private void UpDissolve()
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
    }*/

/*    private IEnumerator DownDissolveValue()
    {
        do
        {
            //DownDissolve();
            //UpDissolve();
            yield return new WaitForSeconds(.01f);
        }
        while (elapsedTime < pressLength);
    }*/

/*    private IEnumerator UDissolveValue()
    {
        do
        {
            UpDissolve();
            yield return new WaitForSeconds(.01f);
        }
        while (elapsedTime < pressLength);
    }*/
} 