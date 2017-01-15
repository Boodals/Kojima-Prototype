//Author:       Yams
//Description:  Add to the car that's meant to be jumping off the ramp.
//Last Edit:    Yams @ 15/01/2017

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RampJumpScript : MonoBehaviour
{   
    public float            HangTime { get { return m_hangTime.Elapsed(); } }
    public int              Score { get { return Mathf.RoundToInt(HangTime * 100);  } }
    public PlayerHUDScript    mRef_playerHUD;

    private CarScript   mRef_carScript;
    private bool        m_flying;
    private Timer       m_hangTime;


	// Use this for initialization
	void Start ()
    {
        mRef_carScript = GetComponent<CarScript>();
        m_flying = false;

        m_hangTime = new Timer();
        m_hangTime.Pause();

        
    }
	
	// Update is called once per frame
	void Update ()
    {
        mRef_playerHUD.DisplayScore(Score);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Ramp")
        {
            m_flying = true;
            m_hangTime.Restart();
            VMadnessController.Singleton.JumpStart();
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (m_flying && other.gameObject.tag == "Island")
        {
            m_flying = false;
            m_hangTime.Pause();
            VMadnessController.Singleton.JumpComplete(Score);
        }
    }
 
}
