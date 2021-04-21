using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerPaint : MonoBehaviour {

    private Renderer rend;
    private AudioSource source;

    public bool erase;
    public static Color eraseColor = new Color(0, 0, 0, 0);

	void Start () {
        rend = this.GetComponent<Renderer>();
        source = this.GetComponent<AudioSource>();
	}

    private void OnCollisionEnter(Collision collision)
    {
        FingerTipCollider ftc = collision.gameObject.GetComponent<FingerTipCollider>();

        if (ftc != null && ftc.isTip)
        {
            ftc.PaintColor = erase ? eraseColor : rend.material.color;
            if (source)
            {
                source.pitch = Random.Range(0.8f, 1.2f);
                source.Play();
            }
        }
    }
}
