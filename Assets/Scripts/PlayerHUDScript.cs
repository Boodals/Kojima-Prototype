using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Bam
{
    public class PlayerHUDScript : MonoBehaviour
    {
        [Header("Countdown Variables")]
        public TrafficLightScript[] myLights;
        public Text countdownTxt;

        float countdownTimer;
        int countdownNum = 3;
        Color startColor;

        [Header("Score Variables")]
        public Text scoreTxt;

        [Header("Timer Variables")]
        public Text timeTxt;

        [Header("Item Variables")]
        public Text itemTxt;

        // Use this for initialization
        void Start()
        {
            countdownTxt.text = "3";
            startColor = countdownTxt.color;
        }

        // Update is called once per frame
        void Update()
        {
            countdownTimer -= Time.deltaTime;

            countdownTxt.transform.localScale = Vector3.Lerp(countdownTxt.transform.localScale, Vector3.zero, Time.deltaTime);

            if (countdownTimer <= 0)
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

        public void DisplayScore(int score)
        {
            scoreTxt.text = "" + score;
        }

        public void DisplayTimer(int mins, int seconds)
        {
            timeTxt.text = "" + mins + ":" + seconds;
        }

        public void ToggleLights(bool areViewable)
        {
            if (areViewable)
            {
                myLights[0].gameObject.SetActive(true);
                myLights[1].gameObject.SetActive(true);
            }
            else
            {
                myLights[0].gameObject.SetActive(false);
                myLights[1].gameObject.SetActive(false);
            }
        }

        public void ShowNextItem(string iconName)
        {
            itemTxt.text = iconName;

            //will later add functionality to change icon images for items
        }

    }
}