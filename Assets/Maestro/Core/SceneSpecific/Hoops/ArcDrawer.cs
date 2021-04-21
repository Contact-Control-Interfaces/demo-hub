using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcDrawer : MonoBehaviour
{
    public float Radius = 7.24f; //NBA
    public float Distance = 0.0f;
    public float Ground = 0.0f;
    public float width;
    public Transform Actor;
    public Transform Hoop;
    public bool isWithin;
    public int numPoints;

    public bool crossHatch;

    //private LineRenderer line2;

    private LineRenderer line;

    // Start is called before the first frame update
    void Start()
    {
        line = this.GetComponent<LineRenderer>();
        line.startWidth = line.endWidth = width;

        /*if (crossHatch)
        {
            line2 = this.gameObject.AddComponent<LineRenderer>();
            line2.material = line.material;
            line2.startWidth = line2.endWidth = width;
            line2.receiveShadows = line.receiveShadows;
            line2.shadowCastingMode = line.shadowCastingMode;
            line2.generateLightingData = line.generateLightingData;
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        CalculateArc();

        if (Actor)
        {
            Vector3 pos = Actor.position;
            isWithin = (new Vector3(pos.x - Hoop.position.x, 0, pos.z - Hoop.position.z)).magnitude <= Radius;
        }
    }

    

    private void CalculateArc()
    {
        List<Vector3> result = new List<Vector3>();

        Vector3 here = Hoop.position + (-Distance * Vector3.forward);

        float offset = Mathf.PI / (numPoints-1); //only half of the unit circle

        for (int i = 0; i < numPoints; i++)
        {
            result.Add(getPoint(offset * i, here));
        }

        if (crossHatch)
        {
            result.Add(result[0]);
            result.Add(result[result.Count - 2]);
        }


        if (Distance > 0)
        {
            result.Insert(0, new Vector3(result[0].x, Ground, result[0].z + Distance));

            Vector3 last = result[result.Count - 1];
            result.Add(new Vector3(last.x, Ground, last.z + Distance));
        }


        line.positionCount = result.Count;
        line.SetPositions(result.ToArray());
    }

    private Vector3 getPoint(float offset, Vector3 here)
    {
        return new Vector3((-Radius * Mathf.Cos(offset)) + here.x, Ground, (-Radius * Mathf.Sin(offset)) + here.z);
    }
}
