using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableRaiser : MonoBehaviour
{
    public float speed = 0.2f;

    private static string inputName = "TableHeight";
    private bool inputBound;

    private Rigidbody rb;

    void Start()
    {
        inputBound = true;

        try {
            Input.GetAxis(inputName);
        } catch (ArgumentException ae) {
            Debug.LogWarning(string.Format("Input {0} is not bound! Define it to raise/lower the table", inputName));
            inputBound = false;
        }
    }

    void Update()
    {
        if (inputBound) {
            float axis = Input.GetAxis(inputName);

            this.gameObject.transform.localPosition += axis * speed * Time.deltaTime * new Vector3(0, 1, 0);
        }
    }
}
