using System.Collections;
using System.Collections.Generic;
using TMPro;
//using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class TextType : MonoBehaviour
{

    public TextMeshProUGUI promptText;
    public int stateNum = 0;
    public float typingSpeed = 0.04f;
    public AudioSource textAudio;

    public Coroutine textCoroutine = null;



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
        TextGen("Neeeaarr... faarr.. WHEREVER YOU ARE!");
        //StartCoroutine(DisplayLine("Neeeaarr... faarr.. WHEREVER YOU ARE!"));
    }

    public void BackText()
    {
        TextGen("Short text is nice :)");
        //StartCoroutine(DisplayLine("Short text is nice :)"));
    }

    public IEnumerator DisplayLine(string line)
    {
        promptText.text = "";

        foreach(char letter in line.ToCharArray())
        {
            textAudio.Play();
            promptText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
       
        textCoroutine = null;
        Debug.Log(textCoroutine);
    }

    public void TextGen(string text)
    {
        if (textCoroutine == null)
        {
            Debug.Log(textCoroutine);
            textCoroutine = StartCoroutine(DisplayLine(text));
        }
    }
}
