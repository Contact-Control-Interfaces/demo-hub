using System.Collections;
using System.Collections.Generic;
using TMPro;
//using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class TextType : MonoBehaviour
{

    public TextMeshProUGUI promptText;
    public List<string> promptTexts = new List<string>();
    public int stateNum = 0;
    public float typingSpeed = 0.04f;




    public void Update()
    {
       if(Input.GetKeyUp(KeyCode.N))
        {
            UpText();
        }
        else if (Input.GetKeyUp(KeyCode.M))
        {
            BackText();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        promptText.text = "";
    }


    public void UpText()
    {
        StartCoroutine(DisplayLine(promptText.text = promptTexts[stateNum]));
        stateNum++;
    }

    public void BackText()
    {
        StartCoroutine(DisplayLine(promptText.text = promptTexts[stateNum]));
        stateNum++;
    }

    public IEnumerator DisplayLine(string line)
    {
        promptText.text = "";

        foreach(char letter in line.ToCharArray())
        {
            promptText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public void TextGen(string text)
    {
        StartCoroutine(DisplayLine(text));
    }
}
