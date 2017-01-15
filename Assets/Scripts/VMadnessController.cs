//Author:       Yams
//Description:  Script to help set up a game of Volcano Madness.
//Last Edit:    Yams @ 15/01/2017

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class VMadnessController : MonoBehaviour
{
    public enum STATE
    {
        NO_STATE,
        INTRO,
        COUNTDOWN,
        PLAY,
        GAME_OVER
    }

    private STATE                       m_state, m_prevState;
    public Camera                       m_introCamera;
    public Animator                     m_introCameraAnimator;
    /// <summary>
    /// Time left before game is over
    /// </summary>
    public float TimeLeft { get { return m_maxTime- m_gameTimer.Elapsed();  } }
    public int                          JumpCar { get; set; }
    public static VMadnessController    Singleton { get { return m_singleton; } }
    private static VMadnessController   m_singleton;
    private Timer                       m_gameTimer;
    [SerializeField]
    private float                       m_maxTime;
    public float                        MaxGameTime { get { return m_maxTime; } }

    private bool                        m_gameOver;
    private int                         m_score = 0;



    public void JumpComplete(int score)
    {
        m_score = score;
        m_state = STATE.GAME_OVER;
    }

    public void JumpStart()
    {
        m_gameTimer.Pause();
        //@Make timer flash / make it obvious left over time -> points
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


        m_state = STATE.INTRO;
        m_prevState = STATE.NO_STATE;

        ///*Hide the player HUDS*/
        //foreach (var hud in MainHUDScript.singleton.playerHUDs) hud.gameObject.SetActive(false);
        ///*Cause transition*/
        CameraManagerScript.singleton.SetupThirdPersonForAllPlayers();

        /*Display correct time*/
        int minutes = Mathf.FloorToInt(MaxGameTime / 60);
        int seconds = Mathf.FloorToInt(MaxGameTime % 60);
        MainHUDScript.singleton.UpdateTimer(minutes, seconds);

        /*Start all cars as stationary*/
        GameController.singleton.AllCarsCanMove(false);
    }
	
	// Update is called once per frame
	void Update ()
    {
        
        //Handle state transition
        if (m_prevState != m_state)
        {
            switch (m_state)
            {
                case STATE.INTRO:       IntroTransition();      break;
                case STATE.COUNTDOWN:   CountdownTransition();  break;
                case STATE.PLAY:        PlayTransition();       break;
                case STATE.GAME_OVER:   GameOverTransition();   break;
            }
        }
        //Record previous state
        m_prevState = m_state;

        //Handle state update
        switch (m_state)
        {
            case STATE.INTRO:       IntroUpdate();      break;
            case STATE.COUNTDOWN:   CountdownUpdate();  break;
            case STATE.PLAY:        PlayUpdate();       break;
            case STATE.GAME_OVER:   GameOverUpdate();   break;
        }
    }


    private void IntroTransition()
    {
        //@Intro init stuff here.
        //@skip this state for now
        //m_introCameraAnimator.SetTrigger("START");
        m_state = STATE.COUNTDOWN;
    }
    private void IntroUpdate()
    {
        //@Add intro update stuff here!
        //m_state = STATE.COUNTDOWN when you're done with intro.
        //AnimatorStateInfo stateInfo= m_introCameraAnimator.GetCurrentAnimatorStateInfo(0);
       //if (stateInfo.IsName("End"))
       // {
         //   m_introCamera.gameObject.SetActive(false);
        //    m_state = STATE.COUNTDOWN;
         //   Debug.Log("blep");
       // }
    }

    private void CountdownTransition()
    {
        //CameraManagerScript.singleton.SetupThirdPersonForAllPlayers();
        foreach (var hud in MainHUDScript.singleton.playerHUDs) hud.gameObject.SetActive(true);
        m_gameTimer.Restart();
    }
    private void CountdownUpdate()
    {
        if (m_gameTimer.Elapsed() >= 6.0f)
        {
            m_state = STATE.PLAY;
        }
    }

    private void PlayTransition()
    {
        m_gameTimer.Restart();
        GameController.singleton.AllCarsCanMove(true);
    }

    private void PlayUpdate()
    {
        if (m_gameTimer.Elapsed() >= MaxGameTime)
        {
            JumpComplete(0);
        }
        else
        {
            int minutes = Mathf.FloorToInt((MaxGameTime - m_gameTimer.Elapsed()) / 60);
            int seconds = Mathf.FloorToInt((MaxGameTime - m_gameTimer.Elapsed()) % 60);
            MainHUDScript.singleton.UpdateTimer(minutes, seconds);
        }
    }

    private void GameOverTransition()
    {
        MainHUDScript.singleton.UpdateTimer(0, 0);
        GameController.singleton.AllCarsCanMove(false);
    }

    private void GameOverUpdate()
    {
        //Display score to cars && give option to quit / continue
        if (Input.anyKeyDown) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
 
}
