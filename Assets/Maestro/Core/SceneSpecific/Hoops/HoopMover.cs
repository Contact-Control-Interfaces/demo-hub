using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoopMover : MonoBehaviour {

    public SliderOutput slider;
    public Transform hoop;
    public Transform start;
    public Transform end;

    private float last;

	// Use this for initialization
	void Start () {
        this.last = slider.outputNormal;
	}
	
	// Update is called once per frame
	void Update () {
        if (slider.outputNormal != this.last)
        {
            hoop.transform.position = Vector3.Lerp(start.transform.position, end.transform.position, slider.outputNormal);
        }

        this.last = slider.outputNormal;
	}
}
