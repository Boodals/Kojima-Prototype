//Author:       Yams
//Description:  Script to help set up a game of Volcano Madness.
//Last Edit:    Yams @ 15/01/2017

using UnityEngine;
using System.Collections;

public class VMadnessController : MonoBehaviour
{
    public int JumpCar { get; set; }
    public static VMadnessController Singleton { get { return m_singleton; } }
    private static VMadnessController m_singleton;
  
    private void Awake()
    {

    }

    // Use this for initialization
    void Start ()
    {
        RampJumpScript ref_jumpScript = GameController.singleton.players[JumpCar].gameObject.AddComponent<RampJumpScript>();
        ref_jumpScript.mRef_playerHUD = MainHUDScript.singleton.playerHUDs[JumpCar];
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
