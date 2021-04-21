using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoopGenerator : MonoBehaviour
{
    public float hoopRadius, radius = 0.1f;
    public int howMany = 5;

    private SphereCollider[] ring;

    // Start is called before the first frame update
    void Start()
    {
        ring = new SphereCollider[howMany];
        float offset = (Mathf.PI * 2) / howMany;
        for (int i = 0; i < howMany; i++)
        {
            float rads = offset * i;
            SphereCollider temp = this.gameObject.AddComponent<SphereCollider>();
            temp.radius = radius;
            temp.center = new Vector3(Mathf.Sin(rads), 0, Mathf.Cos(rads)) * hoopRadius;
            ring[i] = temp;
        }
    }
}
