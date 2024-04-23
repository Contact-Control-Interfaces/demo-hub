using Maestro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class FindSustainedVibration : MonoBehaviour
{
    public bool Search = false;
    private bool lastSearch = false;

    public MaestroInteractable[] Found;

    private void Update()
    {
        if (Search && !lastSearch)
        {
            Found = SearchScene();
        }

        lastSearch = Search;
    }

    private MaestroInteractable[] SearchScene()
    {
        MaestroInteractable[] interactables = GameObject.FindObjectsOfType<MaestroInteractable>();
        return interactables.Where(x => x.stayHaptics.Vibration != null && x.stayHaptics.Vibration.Value != 0).ToArray();
    }
}
