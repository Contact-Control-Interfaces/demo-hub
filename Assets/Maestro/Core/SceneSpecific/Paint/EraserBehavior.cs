using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EraserBehavior : MonoBehaviour {

    public Transform Tableau;
    private List<GameObject> toDestroy;

    private void Start()
    {
        toDestroy = new List<GameObject>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Tableau != null && other.transform.parent.Equals(Tableau))
        {
            other.gameObject.SetActive(false);
            toDestroy.Add(other.gameObject);
        }
    }

    public void Update()
    {
        foreach (GameObject go in toDestroy)
        {
            Destroy(go);
        }
        toDestroy.Clear();
    }
}
