using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeOnComplete : MonoBehaviour {

    public HandScanner left, right;
    public Transform keypad;
    public TextMesh code;
    public KeypadInput keypadInput;

    private Vector3 startLeft, startRight;

    public bool merged, lastMerging, merging;

    private float elapsed;
    public float mergeTime = 1.0f;
    public float mergeDistance = 3.0f;

    private bool LockDefined = false;

	// Use this for initialization
	void Start () {
        try
        {
            Input.GetButton("Lock");
            LockDefined = true;
        } catch (Exception)
        {
            Debug.LogWarning("Input 'Lock' is not bound! Define it for a shortcut to pull up hand scanner.");
            LockDefined = false;
        }

        startLeft = left.transform.localPosition;
        startRight = right.transform.localPosition;
        Reset();
	}

    public void Reset()
    {
        keypadInput.Reset();
        for (int i = 0; i < left.transform.childCount; i++)
        {
            left.transform.GetChild(i).gameObject.SetActive(true);
        }
        left.Reset();
        left.transform.localPosition = startLeft;

        for (int i = 0; i < right.transform.childCount; i++)
        {
            right.transform.GetChild(i).gameObject.SetActive(true);
        }
        right.Reset();
        right.transform.localPosition = startRight;

        //left.transform.localPosition = startLeft;
        //right.transform.localPosition = startRight;
        keypad.gameObject.SetActive(false);
        code.gameObject.SetActive(false);
        merged = merging = lastMerging = false;
        elapsed = mergeTime + 1f;
    }

    // Update is called once per frame
    void Update () {
        elapsed += Time.deltaTime;

        if (LockDefined && Input.GetButton("Lock") && merged)
        {
            Reset();
        }
        else
        {

            if (!(merging || merged) && left.hasScanned && right.hasScanned)
            {
                //Start
                elapsed = 0f;
                merging = true;

                for (int i = 0; i < left.transform.childCount; i++)
                {
                    left.transform.GetChild(i).gameObject.SetActive(false);
                }

                for (int i = 0; i < right.transform.childCount; i++)
                {
                    right.transform.GetChild(i).gameObject.SetActive(false);
                }

            }
            else if (merging && !merged)
            {
                //lerp to center
                left.transform.localPosition = Vector3.Lerp(startLeft, startLeft - new Vector3(mergeDistance, 0, 0), elapsed / mergeTime);
                right.transform.localPosition = Vector3.Lerp(startRight, startRight + new Vector3(mergeDistance, 0, 0), elapsed / mergeTime);

                if (elapsed / mergeTime >= 1f)
                {
                    left.transform.localPosition = startLeft - new Vector3(mergeDistance, 0, 0);
                    right.transform.localPosition = startRight + new Vector3(mergeDistance, 0, 0);
                    merged = true;
                    merging = false;
                    /*keypad.gameObject.SetActive(true);
                    code.gameObject.SetActive(true);

                    string temp = "";
                    for (int i = 0; i < 4; i++)
                    {
                        temp += Random.Range(0, 9);
                    }
                    code.text = temp;
                    keypadInput.code = temp;*/
                    keypadInput.onCodeEntry.Invoke();
                }
            }
        }
	}
}
