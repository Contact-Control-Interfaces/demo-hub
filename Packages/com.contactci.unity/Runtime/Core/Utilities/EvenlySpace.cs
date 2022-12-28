using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class EvenlySpace : MonoBehaviour
{
    public GameObject[] toSpace;

    public float total;
    private float lastTotal;

    public Transform start;

    public Vector3 dir;

    public bool active;
    private bool lastActive;

    void Update()
    {
        if (active && !lastActive || lastTotal != total) {
            float offset = total / (toSpace.Length - 1);

            for (int i = 0; i < toSpace.Length; i++) {
                toSpace[i].transform.position = start.position + (i * offset * dir.normalized);
            }
        }

        lastTotal = total;
        lastActive = active;
    }
}
