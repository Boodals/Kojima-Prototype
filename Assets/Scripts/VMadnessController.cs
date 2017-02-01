//Author:       Yams
//Description:  Script to help set up a game of Volcano Madness.
//Last Edit:    Yams @ 15/01/2017

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Bam
{
    public class VMadnessController : MonoBehaviour
    {
        public float MaxGameTime { get { return m_maxTime; } }
        public static VMadnessController Singleton { get { return m_singleton; } }
        /// <summary>
        /// Time left before game is over
        /// </summary>
        public float TimeLeft { get { return m_maxTime - m_gameTimer.Elapsed(); } }
        public int JumpCar { get; set; }
        /// <summary>
        /// Assign [0] to attackers spawn point
        /// </summary>
        public Transform[] m_spawnPoints;

        public float m_HUDCountdownDuration = 6.0f;
        public float m_transitionDuration = 2.0f;
        public Camera m_introCamera;
        public Animator m_introCameraAnimator;
        [SerializeField]
        private float m_maxTime;

        private static VMadnessController m_singleton;
        private Timer m_gameTimer;
        private bool m_gameOver;
        private int[] m_score;
        private STATE m_state, m_prevState;


        public enum STATE
        {
            NO_STATE,
            INTRO,
            COUNTDOWN,
            PLAY,
            GAME_OVER
        }

        public void JumpComplete(int score)
        {
            m_score[JumpCar] = score;
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

            m_score = new int[4];
            m_gameTimer = new Timer();
            //m_gameTimer.Pause();

            m_singleton = this;
            //DontDestroyOnLoad(this.gameObject);
        }

        // Use this for initialization
        void Start()
        {
            RampJumpScript ref_jumpScript = Kojima.GameController.s_singleton.m_players[JumpCar].gameObject.AddComponent<RampJumpScript>();
            ref_jumpScript.mRef_playerHUD = MainHUDScript.singleton.playerHUDs[JumpCar];

            m_state = STATE.INTRO;
            m_prevState = STATE.NO_STATE;

            //Index for loops
            int idx = 0;
            /*Move cars to correct locations*/
            foreach (var car in Kojima.GameController.s_singleton.m_players)
            {
                if (car.m_nplayerIndex == JumpCar)
                {
                    car.transform.position = m_spawnPoints[0].position;
                    car.transform.rotation = m_spawnPoints[0].rotation;
                }
                else
                {
                    car.transform.position = m_spawnPoints[idx].position;
                    car.transform.rotation = m_spawnPoints[idx].rotation;
                    ++idx;
                }
            }

            idx = 0;
            ///*Hide the player HUDS and update scores*/
            foreach (var hud in MainHUDScript.singleton.playerHUDs)
            {
                hud.DisplayScore(m_score[idx++]);
                hud.gameObject.SetActive(false);
            }
            Kojima.CameraManagerScript.singleton.gameObject.SetActive(false);

            //CameraManagerScript.ScreenSetup introSetup = new CameraManagerScript.ScreenSetup(1);
            //CameraManagerScript.singleton.NewScreenSetup(introSetup);

            /*Display correct time*/
            int minutes = Mathf.FloorToInt(MaxGameTime / 60);
            int seconds = Mathf.FloorToInt(MaxGameTime % 60);
            MainHUDScript.singleton.UpdateTimer(minutes, seconds);

            /*Start all cars as stationary*/
            Kojima.GameController.s_singleton.AllCarsCanMove(false);
        }

        // Update is called once per frame
        void Update()
        {

            //Handle state transition
            if (m_prevState != m_state)
            {
                switch (m_state)
                {
                    case STATE.INTRO: IntroTransition(); break;
                    case STATE.COUNTDOWN: CountdownTransition(); break;
                    case STATE.PLAY: PlayTransition(); break;
                    case STATE.GAME_OVER: GameOverTransition(); break;
                }
            }
            //Record previous state
            m_prevState = m_state;

            //Handle state update
            switch (m_state)
            {
                case STATE.INTRO: IntroUpdate(); break;
                case STATE.COUNTDOWN: CountdownUpdate(); break;
                case STATE.PLAY: PlayUpdate(); break;
                case STATE.GAME_OVER: GameOverUpdate(); break;
            }
        }


        private void IntroTransition()
        {
            //@Intro init stuff here.
            //@skip this state for now
            m_introCameraAnimator.SetTrigger("START");
            m_introCameraAnimator.SetFloat("SPEED", 0.5f);
            //m_state = STATE.COUNTDOWN;
        }

        private void IntroUpdate()
        {
            //@Add intro update stuff here!
            //m_state = STATE.COUNTDOWN when you're done with intro.
            AnimatorStateInfo stateInfo = m_introCameraAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("End"))
            {
                m_state = STATE.COUNTDOWN;
            }
            //Let players skip intro
            else if (Input.anyKeyDown)
            {
                m_state = STATE.COUNTDOWN;
            }
        }

        private void CountdownTransition()
        {
            m_introCamera.enabled = false;
            m_introCamera.gameObject.SetActive(false);
            Kojima.CameraManagerScript.singleton.gameObject.SetActive(true);
            Kojima.CameraManagerScript.singleton.SetupThirdPersonForAllPlayers();
            StartCoroutine(ShowHUD());
            m_gameTimer.Restart();
        }

        IEnumerator ShowHUD()
        {
            yield return new WaitForSeconds(m_transitionDuration);
            foreach (var hud in MainHUDScript.singleton.playerHUDs) hud.gameObject.SetActive(true);
        }

        private void CountdownUpdate()
        {
            if (m_gameTimer.Elapsed() >= m_HUDCountdownDuration + m_transitionDuration)
            {
                m_state = STATE.PLAY;
            }
        }

        private void PlayTransition()
        {
            m_gameTimer.Restart();
            Kojima.GameController.s_singleton.AllCarsCanMove(true);
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
            Kojima.GameController.s_singleton.AllCarsCanMove(false);

            //Disable ramp jump script for current car
            Kojima.GameController.s_singleton.m_players[JumpCar].GetComponent<RampJumpScript>().enabled = false;
            //@some issues here so disable for now
            ////Start with next player
            //if (++JumpCar >= 4)
            //{
            //    //Quit if all players have jumped
            //    Debug.Log("All cars have jumped!");
            //    Application.Quit();
            //}
        }

        private void GameOverUpdate()
        {
            //Display score to cars && give option to quit / continue

            if (Input.anyKeyDown) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

    }
}