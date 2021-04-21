using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WhenClose : MonoBehaviour {

    public bool Ignore = false;
    public float closeness = 0.1f;
    public FaceCenter scanner;
    public UnityEvent onClose;

	// Use this for initialization
	void Start () {
		
	}

    /*private void OnEnable()
    {
        Update();
    }*/

    // Update is called once per frame
    void Update () {
        if (!Ignore)
        {
            bool close = scanner.DistanceToTarget < closeness;
            if (close && onClose != null)
            {
                onClose.Invoke();
            }
        }
        
	}

    private void OnDisable()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void TurnOnChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }
}
