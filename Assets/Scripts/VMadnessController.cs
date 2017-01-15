//Author:       Yams
//Description:  Script to help set up a game of Volcano Madness.
//Last Edit:    Yams @ 15/01/2017

using UnityEngine;
using System.Collections;

public class VMadnessController : MonoBehaviour
{
    public int                          JumpCar { get; set; }
    public static VMadnessController    Singleton { get { return m_singleton; } }
    private static VMadnessController   m_singleton;
    private Timer m_gameTimer;
    [SerializeField]
    private float m_maxTime;
    public float MaxGameTime { get { return m_maxTime; } }

    private bool m_gameOver;

    public void JumpComplete(int score)
    { 
        foreach (var car in GameController.singleton.players)
        {
            car.CanMove = false;
        }
        //@Game over screen
    }

    public void JumpStart()
    {
        m_gameTimer.Pause();
    }

    public void GameStart()
    {
        m_gameTimer.Restart();
    }

    private void Awake()
    {
        if (m_singleton != null) return;

        m_gameTimer = new Timer();
        //m_gameTimer.Pause();

        m_singleton = this;
    }

    // Use this for initialization
    void Start ()
    {
        RampJumpScript ref_jumpScript = GameController.singleton.players[JumpCar].gameObject.AddComponent<RampJumpScript>();
        ref_jumpScript.mRef_playerHUD = MainHUDScript.singleton.playerHUDs[JumpCar];
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (m_gameOver) return;

        if (m_gameTimer.Elapsed() >= MaxGameTime)
        {
            MainHUDScript.singleton.UpdateTimer(0, 0);
            m_gameOver = true;
            JumpComplete(0);
        }
        else
        {
            int minutes = Mathf.RoundToInt((MaxGameTime - m_gameTimer.Elapsed()) / 60);
            int seconds = Mathf.RoundToInt((MaxGameTime - m_gameTimer.Elapsed()) % 60);
            MainHUDScript.singleton.UpdateTimer(minutes, seconds);
        }
    }
}
