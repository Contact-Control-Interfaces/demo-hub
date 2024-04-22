using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DemoConfiguration
{
    public bool? IsSingleHand;
    public List<string> DemoScenes;
    public string StartingScene;

    public static DemoConfiguration CreateFromJSON(string configJson)
    {
        return JsonUtility.FromJson<DemoConfiguration>(configJson);
    }

}
