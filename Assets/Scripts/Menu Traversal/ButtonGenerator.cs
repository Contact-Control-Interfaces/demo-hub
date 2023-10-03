using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonGenerator : MonoBehaviour
{
    public GameObject baseButton;
    public DemoPress currentButton;
    public List<DemoPress> demoButtons = new List<DemoPress>();

    public void GenerateButton(string sceneName, Sprite demoSprite, Material buttonMaterials)
    {
        currentButton = Instantiate(baseButton, this.transform).GetComponent<DemoPress>();
        
        currentButton.name = currentButton.SceneName = sceneName;
        currentButton.demoSprite = demoSprite;
        currentButton.buttonMaterial = buttonMaterials;

        demoButtons.Add(currentButton);
    }
}
