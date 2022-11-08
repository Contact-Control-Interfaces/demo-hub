using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class TextType : MonoBehaviour
{

    public TextMeshProUGUI promptText;
    public List<string> promptTexts = new List<string>();
    public int stateNum = 0;
    public float typingSpeed = 0.04f;

    public enum DEMO_STATE
    {
        START,
        HAND_OUT,
        HOLD_STILL,
        BITE,
        MENU
    }
    public DEMO_STATE state = DEMO_STATE.START;


    public void Update()
    {
       if(Input.GetKeyUp(KeyCode.N))
        {
            NextState();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        promptTexts.Add("Welcome");
        promptTexts.Add("Put your hand out under the apple.");
        promptTexts.Add("Please hold still...");
        promptTexts.Add("Apple Dropped.");

    }

    public void NextState()
    {
        switch (state)
        {
            case DEMO_STATE.START:
                UpNum();
                state = DEMO_STATE.HAND_OUT;
                break;
            case DEMO_STATE.HAND_OUT:
                UpNum();
                state = DEMO_STATE.HOLD_STILL;
                break;
            case DEMO_STATE.HOLD_STILL:
                UpNum();
                state = DEMO_STATE.BITE;
                break;
            case DEMO_STATE.BITE:
                UpNum();
                state = DEMO_STATE.MENU;
                break;
            case DEMO_STATE.MENU:
                UpNum();
                break;
            default:
                break;
        }
    }

    private void UpNum()
    {
        StartCoroutine(DisplayLine(promptText.text = promptTexts[stateNum]));
        stateNum++;
    }

    private IEnumerator DisplayLine(string line)
    {
        promptText.text = "";

        foreach(char letter in line.ToCharArray())
        {
            promptText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }




}
