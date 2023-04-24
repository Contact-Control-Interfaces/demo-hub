using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class Micro_Info
{
    public string demoName;

    public string demoIconPath;

    public string demoScenePath;

    /*    public string JSONResult()
        {
            string result = JsonUtility.ToJson(this);
            File.WriteAllText("Assets/Intro-Hub-Files/Scripts/JSON/test.json", result);

            return result;
        }*/

}
