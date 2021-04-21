using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BallDetector : MonoBehaviour {

    public new ParticleSystem particleSystem;
    private AudioSource source;

    private List<MaestroInteractable> watching;

    public UnityEvent onScore;

    private int combo = 0;
    private const float opacity = 0.1f;
    private Color[] colors = new Color[] {
        //new Color(0xff, 0x00, 0x00),
        //new Color(0x00, 0xff, 0x00),
        //new Color(0x00, 0x00, 0xff),
        new Color(0xbd / 255f, 0x69 / 255f, 0xd4 / 255f, opacity),  // Violet
        new Color(0xff / 255f, 0xa5 / 255f, 0xe4 / 255f, opacity),  // Pink
        new Color(0x87 / 255f, 0xd1 / 255f, 0xd0 / 255f, opacity),  // Contact Blue
        new Color(0x6c / 255f, 0xc4 / 255f, 0x70 / 255f, opacity),  // Green
        new Color(0xf5 / 255f, 0xe0 / 255f, 0x47 / 255f, opacity),  // Yellow
        new Color(0xde / 255f, 0x92 / 255f, 0x4a / 255f, opacity),  // Orange
        new Color(0xca / 255f, 0x4d / 255f, 0x3a / 255f, opacity)   // Red
    };

    private float scoreDelay = 0.5f, elapsed = 0f;

    private float minSpeed = 0.3f;

    private int ignoreLayer;

    void Start () {
        watching = new List<MaestroInteractable>();
        if (onScore == null)
            onScore = new UnityEvent();

        source = GetComponent<AudioSource>();

        combo = 0;

        ignoreLayer = LayerMask.NameToLayer("IgnoreNet");

    }
	
	// Update is called once per frame
	void Update () {
		foreach (MaestroInteractable mi in watching)
        {
            if (mi.transform.position.y <= this.transform.position.y && elapsed > scoreDelay)
            {
                particleSystem.Clear();

                elapsed = 0f;

                //Set combo stuff
                int index = combo < colors.Length ? combo: colors.Length - 1;
                particleSystem.GetComponent<Renderer>().material.color = colors[index];
                var main = particleSystem.main;
                main.maxParticles = 800 * (index+1);

                particleSystem.Play();
                source.Play();
                onScore.Invoke();

                //TODO check misses
                combo++;

                ComboResetter cr = mi.GetComponent<ComboResetter>();
                if (cr != null)
                    cr.hasScored = true;
            }
        }

        elapsed += Time.deltaTime;
	}

    private void OnTriggerEnter(Collider other)
    {
        MaestroInteractable temp = other.GetComponent<MaestroInteractable>();
        if (temp != null && temp.gameObject.layer != ignoreLayer){

            //Debug.Log("OH THANK GOD");
            if (!watching.Contains(temp) && temp.transform.position.y > this.transform.position.y)
                watching.Add(temp);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        MaestroInteractable temp = other.GetComponent<MaestroInteractable>();
        if (temp != null && watching.Contains(temp))
        {
            watching.Remove(temp);
        }
    }

    public void ResetCombo()
    {
        combo = 0;
    }
}
