using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConfigJSONHandler : MonoBehaviour
{
    public List<string> demoNamesAvalible;
    public List<Sprite> demoSprites;
    public List<Material> buttonMaterials;

    public ButtonGenerator buttonGen;

    public DemoConfiguration demoTest;

    public string StartScene;

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        if (SceneManager.GetActiveScene().name == "Start Zone")
        {
            ReadConfig();
            SceneManager.LoadScene(demoTest.StartScene);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {

        buttonGen = FindObjectOfType<ButtonGenerator>(true);
        if (buttonGen != null)
        {
            if (buttonGen.demoButtons.Count <= 0)
            {
                GenerateDynamic();
            }
        }
    }

    public void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void ReadConfig()
    {
        if (File.Exists("./config.json"))
        {
            demoTest = DemoConfiguration.CreateFromJSON(File.ReadAllText("./config.json"));
            StartScene = demoTest.StartScene;
        }
    }

    public void GenerateDynamic()
    {
        foreach (string sceneName in demoTest.Scenes)
        {
            for (int i = 0; i < demoNamesAvalible.Count; i++)
            {
                if (sceneName.Equals(demoNamesAvalible[i]))
                {
                    buttonGen.GenerateButton(sceneName, demoSprites[i], buttonMaterials[i]);
                }
            }
        }

        if( buttonGen.demoButtons.Count == 0)
        {
            GenerateDefault();
        }
    }

    public void GenerateDefault()
    {
        for (int i = 0; i < demoNamesAvalible.Count; i++)
        {
            buttonGen.GenerateButton(demoNamesAvalible[i], demoSprites[i], buttonMaterials[i]);
        }
    }
}



