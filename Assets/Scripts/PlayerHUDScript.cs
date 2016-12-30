﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerHUDScript : MonoBehaviour
{
    //COUNTDOWN VARIABLES
    public TrafficLightScript[] myLights;
    public Text countdownTxt;

    float countdownTimer;
    int countdownNum = 3;
    Color startColor;

	// Use this for initialization
	void Start ()
    {
        countdownTxt.text = "3";
        startColor = countdownTxt.color;
    }

    // Update is called once per frame
    void Update ()
    {
        countdownTimer -= Time.deltaTime;

        countdownTxt.transform.localScale = Vector3.Lerp(countdownTxt.transform.localScale, Vector3.zero, Time.deltaTime);

        if(countdownTimer <= 0)
        {
            Countdown();
        }
	}

    void Countdown()
    {
        countdownTxt.transform.localScale = Vector3.one;

        for (int i = 0; i < 2; i++)
        {
            myLights[i].ChangeColour(countdownNum);
        }

        if (countdownNum > 0)
        {         
            countdownTxt.color = startColor;
            countdownTxt.text = "" + countdownNum;
            countdownNum--;
            countdownTimer = 2;            
        }
        else
        {
            countdownTxt.text = "GO";
            countdownTxt.color = Color.Lerp(countdownTxt.color, Color.clear, Time.deltaTime * 2);
        }
    }
}
