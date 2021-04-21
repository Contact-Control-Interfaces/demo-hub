using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CylinderBetween : MonoBehaviour
{
    public Transform a;
    public Transform b;
    public float size;

    private Rigidbody _ar;
    private Rigidbody ar {
        get {
            if (!_ar) _ar = a.GetComponent<Rigidbody>();
            return _ar;
        }
    }

    private Rigidbody _br;
    private Rigidbody br {
        get {
            if (!_br) _br = b.GetComponent<Rigidbody>();
            return _br;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (a && b) {
            transform.position = (a.position + b.position) / 2;
            transform.localScale = new Vector3(size, (b.position - a.position).magnitude / 2, size);
            transform.rotation = Quaternion.LookRotation(Vector3.Cross(b.position - a.position, Vector3.forward), b.position - a.position);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (ar)
            ar.AddForce(-collision.impulse, ForceMode.Impulse);
        if (br)
            br.AddForce(-collision.impulse, ForceMode.Impulse);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (ar)
            ar.AddForce(-collision.impulse, ForceMode.Impulse);

        if (br)
            br.AddForce(-collision.impulse, ForceMode.Impulse);
    }

    /*public void Init()
    {
        Update();
    }*/
}
