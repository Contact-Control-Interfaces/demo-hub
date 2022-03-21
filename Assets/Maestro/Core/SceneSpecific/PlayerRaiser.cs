using System;
using UnityEngine;

public class PlayerRaiser : MonoBehaviour
{
    public Transform toRaise;
    public float speed = 0.2f;

    private static string inputName = "PlayerHeight";
    private bool inputBound;

    void Start()
    {
        if (toRaise == null)
            toRaise = this.transform;

        inputBound = true;

        try {
            Input.GetAxis(inputName);
        } catch (ArgumentException ae) {
            Debug.LogWarning(string.Format("Input {0} is not bound! Define it to raise/lower the player rig", inputName));
            inputBound = false;
        }
    }

    void Update()
    {
        if (inputBound) {
            float axis = Input.GetAxis(inputName);

            toRaise.localPosition += axis * speed * Time.deltaTime * Vector3.up;
        }
    }
}
