using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboResetter : MonoBehaviour
{
    public BallDetector toReset;
    public float yValue;
    public float throwSpeed;

    public bool isChecking;
    public bool hasScored;

    // Start is called before the first frame update
    void Start()
    {
        isChecking = false;
        hasScored = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasScored)
            isChecking = false;

        if (isChecking)
        {
            if (this.gameObject.transform.position.y < yValue)
            {
                toReset.ResetCombo();
                isChecking = false;
            }
        }
    }

    public void CheckIfThrown(MaestroInteractable ball)
    {
        if (ball.GetComponent<Rigidbody>().velocity.magnitude >= throwSpeed)
        {
            isChecking = true;
        }
    }
}
