using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TrafficLightScript : MonoBehaviour
{
    public Image[] glowColours;
    public Vector3 startPos;
    public Vector3 endPos;

    bool showLights;
    float delayTimer = 2;

	// Use this for initialization
	void Start ()
    {
        showLights = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(showLights)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, Time.deltaTime * 5);
        }
        else
        {
            delayTimer -= Time.deltaTime;

            if (delayTimer <= 0)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, endPos, Time.deltaTime * 2);
            }
        }
	}

    public void ChangeColour(int timeStage)
    {
        switch(timeStage)
        {
            case 0:
                glowColours[1].enabled = false;
                glowColours[0].enabled = true;
                MoveLights(1);
                break;
            case 1:
                glowColours[2].enabled = false;
                glowColours[1].enabled = true;
                break;
            case 2:
                glowColours[2].enabled = true;
                break;
            case 3:
                glowColours[2].enabled = true;
                MoveLights(0);
                break;
        }
    }

    void MoveLights(int direction)
    {
        if(direction == 0)
        {
            showLights = true;
        }
        else
        {
            showLights = false;
        }
    }
}
