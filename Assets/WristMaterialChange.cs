using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WristMaterialChange : MonoBehaviour
{
    public int buttonPresses = 0;

    public void MaterialChange(Material buttonMaterial, MeshRenderer buttonRender)
    {
        buttonPresses += 1;
        if (buttonPresses == 1)
        {
            buttonRender.material = buttonMaterial;
        }
    }
}
