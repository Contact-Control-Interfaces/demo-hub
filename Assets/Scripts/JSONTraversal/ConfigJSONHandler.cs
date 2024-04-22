using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    Dictionary<string, DemoContext> context;

    [SerializeField]
    string configLocation = "./config.json";
    public DemoConfiguration configData;

    struct DemoContext 
    {
       public Sprite sprite;
       public Material material;

        public DemoContext(Sprite sprite, Material material)
        {
            this.sprite = sprite;
            this.material = material;
        }
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        context = new Dictionary<string, DemoContext>();

        for (int i = 0; i < demoNamesAvalible.Count; i++)
        {
            context.Add(demoNamesAvalible[i], new DemoContext(demoSprites[i], buttonMaterials[i]));
        }

        if (SceneManager.GetActiveScene().name == "Start Zone")
        {
            ReadConfig();
            SceneManager.LoadScene(configData.StartingScene);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        buttonGen = FindObjectOfType<ButtonGenerator>(true);

        if (buttonGen != null && buttonGen.demoButtons.Count <= 0)
        {
            GenerateDynamic();
        }
    }

    public void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    public void ReadConfig()
    {
        if (File.Exists(configLocation))
        {
            configData = DemoConfiguration.CreateFromJSON(File.ReadAllText(configLocation));
        }
        else
        {
            PopulateConfig();
        }
    }

    public void PopulateConfig()
    {
        Debug.Log("No Config Found, Load Temp Values");
        configData.StartingScene = "AppleTree";
        configData.DemoScenes = new List<string> { "InteractionPanel", "Paint", "Shapes", "VibrationOrbs", "AppleTree" };
        configData.IsSingleHand = false;
    }

    public void GenerateDynamic()
    {
        var avaliableDemo = context.Keys.Intersect(configData.DemoScenes).ToDictionary(x => x, x => context[x]);

        foreach (var demo in avaliableDemo )
        {
            buttonGen.GenerateButton(demo.Key, demo.Value.sprite, demo.Value.material);
        }

        if ( buttonGen.demoButtons.Count == 0)
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



