using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class KeyBindHint : MonoBehaviour
{
    public GameObject keyBinds;

    public void Start()
    {
        DontDestroyOnLoad(this);
    }

    public void ShowKeyBinds()
    {
        keyBinds.SetActive(!keyBinds.activeSelf);
    }
}
