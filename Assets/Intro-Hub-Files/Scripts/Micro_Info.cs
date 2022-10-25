using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class Micro_Info //: MonoBehaviour
{
    
    public string demoName { get; set; }

    public string demoIconPath { get; set; }

    public string demoScenePath { get; set; }


    /*    public string JSONResult()
        {
            string result = JsonUtility.ToJson(this);
            File.WriteAllText("Assets/Intro-Hub-Files/Scripts/JSON/test.json", result);

            return result;
        }*/

}
