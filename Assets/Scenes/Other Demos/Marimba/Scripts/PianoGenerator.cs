using Maestro;
using Maestro.Vibration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoGenerator : KeyboardGenerator
{
    public GameObject BlackKeyPrefab;

    public override bool Generate()
    {
        if ((KeyPrefab ?? BlackKeyPrefab) == null) {
            Debug.LogError("No key prefabs!");
            return false;
        }

        // Remove old Pianos
        var everything = target.GetComponents<Piano>();
        foreach (Piano p in everything) {
            p.enabled = false;
            if (Application.isPlaying) {
                Destroy(p);
            } else {
                DestroyImmediate(p);
            }
        }

        Piano piano = target.gameObject.AddComponent<Piano>();

        if (base.Generate()) {
            piano.whiteKeys = this.naturals.ToArray();
            piano.blackKeys = this.others.ToArray();

            return true;
        }

        return false;
    }
}
