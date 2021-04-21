using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCenter : MonoBehaviour {

    public Camera LeapCamera, KnucklesCamera;

    private Camera face { get { return LeapCamera != null && LeapCamera.isActiveAndEnabled ? LeapCamera : KnucklesCamera; } }
    public TextMesh prompt;
    public float radius = 2f;
    public int smoothing = 1;

    public Vector3? target;
    public float targetWeight = 0.02f;
    public float dotTarget = 0.9f;

    public string turn = "Turn", come = "Come\nhere";
    public bool caps = false;

    private List<Vector3> positions;

    private void Start()
    {
        positions = new List<Vector3>();

    }

    public float DistanceToTarget { get { return target.HasValue ? 
                Vector3.Scale((target.Value - face.transform.position), new Vector3(1,1,0)).magnitude :
                0f; } }

    public void MoveToCenter()
    {
        target = new Vector3(0, face.transform.position.y, 0);
    }

    public void ResetTarget()
    {
        target = null;
    }

    // Update is called once per frame
    void Update () {


        //face.transform.forward;
        //Vector3 pos = face.transform.position + face.transform.forward * radius;

        Vector3 pos;
        if (target.HasValue && Vector3.Dot(face.transform.forward.normalized, (target.Value - face.transform.position).normalized) > dotTarget)
        {
            pos = target.Value;
            prompt.text = caps ? come.ToUpper() : come;
        } else {

             pos = face.transform.position + (target.HasValue ?
                Vector3.RotateTowards(face.transform.forward, (target.Value - face.transform.position), targetWeight, 0f) :
                face.transform.forward) * radius;
            prompt.text = caps ? turn.ToUpper() : turn;
        }
        positions.Add(pos);

        while (positions.Count > smoothing) positions.RemoveAt(0);

        //Vector3 center = new Vector3(0, face.transform.position.y, 0);

        //transform.position = //center + (Vector3.Scale(face.transform.forward, new Vector3(1, 0, 1)) * radius); //  * radius);\
        transform.position = Centroid(positions);
        transform.LookAt(face.transform.position, Vector3.up);

        //if (target.HasValue)
        //{
        //    transform.Translate((target.Value - transform.position).normalized * Time.deltaTime);
        //}
	}

    Vector3 Centroid(List<Vector3> list)
    {
        if (list.Count <= 0)
            return Vector3.zero;
        else if (list.Count < 2)
            return list[0];
        else {

            Vector3 result = Vector3.zero;
            for (int i = 0; i < list.Count; i++)
            {
                result += list[i];
            }

            return Vector3.Scale(result, (1.0f / list.Count) * Vector3.one);
        }
    }
}
