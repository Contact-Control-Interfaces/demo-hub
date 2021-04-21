using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulser : MonoBehaviour {

    private GameObject[] toPulse;
    private int active;

    // Use this for initialization
    private void OnEnable()
    {
        foreach (GameObject go in toPulse)
        {
            go.SetActive(false);
        }

        toPulse[active].SetActive(true);
    }

    private void Awake()
    {
        active = 0;
        toPulse = new GameObject[this.transform.childCount];

        for (int i = 0; i < toPulse.Length; i++)
        {
            toPulse[i] = this.transform.GetChild(i).gameObject;
        }
    }

    public void Tick()
    {
        toPulse[active].SetActive(false);

        active++;
        if (active >= toPulse.Length)
            active = 0;

        toPulse[active].SetActive(true);
    }
}
