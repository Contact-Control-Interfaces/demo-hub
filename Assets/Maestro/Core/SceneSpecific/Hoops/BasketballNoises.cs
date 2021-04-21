using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketballNoises : MonoBehaviour {

    public AudioClip[] samples;
    public AudioClip backboard;

    public PhysicMaterial backboardMaterial;

    private AudioSource source;

	// Use this for initialization
	void Awake () {
        this.source = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.GetComponentInParent<FingerTipCollider>() != null)
            return;
        

        AudioClip toUse;
        if (collision.collider.material.Equals(backboardMaterial))
            toUse = backboard;
        else
            toUse = samples[Random.Range(0, samples.Length)];

        source.clip = toUse;
        source.volume = Mathf.Max(0.01f, Mathf.Min(1.0f, collision.impulse.magnitude * 0.0025f));
        source.Play();
    }
}
