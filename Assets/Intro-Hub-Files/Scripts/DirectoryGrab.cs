using Leap.Unity;
using Leap.Unity.Attributes;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DirectoryGrab : MonoBehaviour
{
    [SerializeField]
    public static Micro_Info curInfo;

    
    static void SubFolderExample()
    {
        //This method prints out the entire folder list of a project into the console
        var folders = AssetDatabase.GetSubFolders("Assets/Intro-Hub-Files/Scripts");
        foreach (var folder in folders)
        {
            if(Recursive(folder).Contains("JSON"))
            {

                DirectoryInfo dir = new DirectoryInfo(folder);
                FileInfo[] info = dir.GetFiles("*.json");
                foreach (FileInfo f in info)
                {
                    string dirName = "Assets/Intro-Hub-Files/Scripts/JSON/" + f.Name;

                    curInfo = JsonUtility.FromJson<Micro_Info>(File.ReadAllText(dirName));

                    //rInfo = JsonUtility.FromJsonOverwrite(File.ReadAllText(dirName);

                    string result = f.Name + " - " + "\nDemo Name: "+ curInfo.demoName + "\nDemo Icon Path:" + curInfo.demoIconPath + "\nDemo Scene Path:" + curInfo.demoScenePath;

                    Debug.Log(result);



                }
            }
        }
    }

    static string Recursive(string folder)
    {
        //Debug.Log(folder);
        var folders = AssetDatabase.GetSubFolders(folder);
        foreach (var fld in folders)
        {
            Recursive(fld);
 
        }

        return folder;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            SubFolderExample();
        }
    }
}
