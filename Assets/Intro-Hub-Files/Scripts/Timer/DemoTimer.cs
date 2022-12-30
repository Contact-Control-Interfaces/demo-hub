using JetBrains.Annotations;
using Maestro.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoTimer : MonoBehaviour
{

    static float timeLeft = 0;
    public TextMeshProUGUI timerText;
    

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        timerText.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
        }

        //1 Minute Override
        if (Input.GetKeyDown(KeyCode.Q))
        {

            timeLeft = 61;

        }

        if (Input.GetKeyDown(KeyCode.W))
        {

            timeLeft = 121;

        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            timeLeft += 61;

        }
        
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            timeLeft += 121;

        }
        
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            timeLeft += 181;

        }
        
        if(Input.GetKeyDown(KeyCode.Alpha4))
        {
            timeLeft += 241;

        }
        
        if(Input.GetKeyDown(KeyCode.Alpha5))
        {
            timeLeft += 301;

        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            timeLeft += 361;

        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            timeLeft += 421;

        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            timeLeft += 481;

        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            timeLeft += 541;

        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            timeLeft += 601;

        }



        if (timeLeft > 1)
        {
            timeLeft -= Time.deltaTime;
            UpdateTimerText();
           
            if(timeLeft < 1)
            {
                //SceneManager.LoadScene(0);
                string timesUp = "Time's almost up!";

                DisplayBLE.SetLeftText(timesUp);
                DisplayBLE.SetRightText(timesUp);

                timerText.text = "Time's Up!";
            }
        }
    }

    public void UpdateTimerText()
    {
        int leftMins;
        float leftSecs;
        leftSecs = timeLeft % 60;
        leftMins = (int)(timeLeft / 60);
        timerText.text = leftMins.ToString("00") + ":" + leftSecs.ToString("00.00");
    }
}
