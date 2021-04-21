using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HandScanner : MonoBehaviour {

    public Transform scanner;
    public Transform check;
    public Transform pulsers;

    private Pulser[] pulse;

    public float scanTime = 1.0f;
    private float elapsedTime = 0.0f;

    public float pulseTime = 1.0f;
    private float pulseElapsed;

    private Vector3 startPosition, endPosition;

    public bool Scanning { get { return elapsedTime < scanTime; } }
    private bool LastScanning;

    public bool hasScanned;

    public bool[] touching = new bool[5];

    public void Scan()
    {
        if (!(Scanning || hasScanned))
        {
            Debug.Log("SCANNING!");
            elapsedTime = 0f;
            /*for (int i = 0; i < scanner.childCount; i++)
            {
                scanner.GetChild(i).gameObject.SetActive(true);
            }*/
            scanner.gameObject.SetActive(true);
        }
    }

    public void Register(FingerTipCollider ftc){ RegisterHelper(ftc, true); }

    public void Unregister(FingerTipCollider ftc) { RegisterHelper(ftc, false); }

    private void RegisterHelper(FingerTipCollider ftc, bool whatever)
    {
        if (ftc.index % 2 == 1 && ftc.index < 10)
        {
            touching[ftc.index / 2] = whatever;
            pulse[ftc.index / 2].gameObject.SetActive(whatever);
        }
    }

    public void Reset()
    {
        this.gameObject.SetActive(true);

        hasScanned = false;
        scanner.gameObject.SetActive(false);
        check.gameObject.SetActive(false);
        

        pulseElapsed = 0;
        startPosition = new Vector3(0, 0.5f, 0.7f);
        endPosition = Vector3.Scale(startPosition, new Vector3(1, -1, 1));
        elapsedTime = 1000.0f;
        touching = new bool[5];
    }

    private void OnEnable()
    {
        if (pulse != null)
        {
            foreach (Pulser p in pulse)
            {
                if (p.gameObject)
                    p.gameObject.SetActive(false);
            }
        }
    }

    // Use this for initialization
    void Start () {
        pulse = pulsers.GetComponentsInChildren<Pulser>(true);

        foreach (Pulser p in pulse)
            p.gameObject.SetActive(false);

        MaestroInteractable mi = this.GetComponent<MaestroInteractable>();

        if (mi.onTouch == null)
            mi.onTouch = new TouchEvent();
        mi.onTouch.AddListener(Register);

        if (mi.unTouch == null)
            mi.unTouch = new TouchEvent();
        mi.unTouch.AddListener(Unregister);

        Reset();
	}

    // Update is called once per frame
    void Update() {
        elapsedTime += Time.deltaTime;
        pulseElapsed += Time.deltaTime;

        if (!(hasScanned || Scanning || LastScanning) && touching[0] && touching[1] && touching[2] && touching[3] && touching[4])
        {
            this.Scan();
        }

        if (pulseElapsed > pulseTime)
        {
            pulseElapsed %= pulseTime;
            foreach (Pulser p in pulse)
                p.Tick();
        }

        if (Scanning)
        {
            float passTime = scanTime / 2f;
            if (elapsedTime < passTime)
            {
                scanner.localPosition = Vector3.Lerp(startPosition, endPosition, elapsedTime / passTime);
            } else
            {
                scanner.localPosition = Vector3.Lerp(endPosition, startPosition, (elapsedTime / passTime) - 1);
            }
        } else if (LastScanning)
        {
            //turn off lights
            /*for (int i = 0; i < scanner.childCount; i++)
            {
                scanner.GetChild(i).gameObject.SetActive(false);
            }*/
            scanner.gameObject.SetActive(false);
            scanner.localPosition = startPosition;
            if (!hasScanned)
            {
                hasScanned = true;
                check.gameObject.SetActive(true);
                //TODO actually check hand
            }
        }

        LastScanning = Scanning;
	}
}
