using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Kojima
{
    public class CameraManagerScript : MonoBehaviour
    {

        //These are for convenience when you set up a new view
        public static bool[] FollowPlayerOne, FollowPlayerTwo, FollowPlayerThree, FollowPlayerFour, FollowAll;

        public static CameraManagerScript singleton;

        [System.Serializable]
        public struct screenSetup_s
        {
            public int cameras;
            public Bam.PlayerCameraScript.CameraInfo[] camInfos;

            public screenSetup_s(int howManyCameras)
            {
                cameras = howManyCameras;
                camInfos = new Bam.PlayerCameraScript.CameraInfo[cameras];

                for (int i = 0; i < camInfos.Length; i++)
                {
                    camInfos[i].m_followThesePlayers = new bool[4];
                }
            }
        }

        public GameObject playerCameraPrefab;
        public Bam.PlayerCameraScript[] playerCameras;

        public Image transitionImg;

        //QUICK SETUPS
        public void SetupLobbyCamera()
        {
            screenSetup_s newSS = new screenSetup_s();
            newSS.cameras = 1;
            newSS.camInfos = new Bam.PlayerCameraScript.CameraInfo[4];

            newSS.camInfos[0].m_followThesePlayers = new bool[4] { true, true, true, true };
            newSS.camInfos[0].m_viewStyle = Bam.PlayerCameraScript.viewStyles_e.overhead;
            newSS.camInfos[0].m_positionOnScreen = Bam.PlayerCameraScript.screenPositions_e.fullScreen;

            NewScreenSetup(newSS);
        }

        public void SetupSingularOverheadView(bool followPlayerOne, bool followPlayerTwo, bool followPlayerThree, bool followPlayerFour)
        {
            screenSetup_s newSS = new screenSetup_s();
            newSS.cameras = 1;
            newSS.camInfos = new Bam.PlayerCameraScript.CameraInfo[4];

            newSS.camInfos[0].m_followThesePlayers = new bool[4] { followPlayerOne, followPlayerTwo, followPlayerThree, followPlayerFour };
            newSS.camInfos[0].m_viewStyle = Bam.PlayerCameraScript.viewStyles_e.overhead;
            newSS.camInfos[0].m_positionOnScreen = Bam.PlayerCameraScript.screenPositions_e.fullScreen;

            NewScreenSetup(newSS);
        }

        public void SetupThirdPersonForAllPlayers()
        {
            screenSetup_s newSS = new screenSetup_s();
            newSS.cameras = GameController.s_ncurrentPlayers;

            newSS.camInfos = new Bam.PlayerCameraScript.CameraInfo[4];

            for (int i = 0; i < 4; i++)
            {
                newSS.camInfos[i].m_followThesePlayers = new bool[4];

                for (int fTP = 0; fTP < 4; fTP++)
                {
                    newSS.camInfos[i].m_followThesePlayers[fTP] = fTP == i;
                }

                newSS.camInfos[i].m_positionOnScreen = (Bam.PlayerCameraScript.screenPositions_e)i;
            }

            if (GameController.s_ncurrentPlayers == 2)
            {
                newSS.camInfos[0].m_positionOnScreen = Bam.PlayerCameraScript.screenPositions_e.topHalf;
                newSS.camInfos[1].m_positionOnScreen = Bam.PlayerCameraScript.screenPositions_e.bottomHalf;
            }

            if (GameController.s_ncurrentPlayers == 1)
            {
                newSS.camInfos[0].m_positionOnScreen = Bam.PlayerCameraScript.screenPositions_e.fullScreen;
            }

            NewScreenSetup(newSS);
        }

        //OTHER PUBLIC FUNCTIONS
        public void NewScreenSetup(screenSetup_s newSetup)
        {
            StopCoroutine("Transition");
            StartCoroutine("Transition", newSetup);
        }


        void Awake()
        {
            singleton = this;
            transitionImg.fillAmount = 1;
            transitionImg.color = Color.white;
        }

        // Use this for initialization
        void Start()
        {
            playerCameras = CreateCameras(GameController.s_ncurrentPlayers);
            //SetupThirdPersonForAllPlayers();

            FollowPlayerOne = new bool[4] { true, false, false, false };
            FollowPlayerTwo = new bool[4] { false, true, false, false };
            FollowPlayerThree = new bool[4] { false, false, true, false };
            FollowPlayerFour = new bool[4] { false, false, false, true };
            FollowAll = new bool[4] { true, true, true, true };

            transitionImg.fillAmount = 1;
            transitionImg.color = Color.white;

            for (int i = 0; i < GameController.s_ncurrentPlayers; i++)
            {
                if (Bam.MainHUDScript.singleton)
                {
                    if (Bam.MainHUDScript.singleton.playerHUDs[i])
                    {
                        Bam.MainHUDScript.singleton.playerHUDs[i].GetComponent<Canvas>().worldCamera = playerCameras[i].GetUICameraComponent();
                        Bam.MainHUDScript.singleton.playerHUDs[i].GetComponent<Canvas>().planeDistance = 0.5f;
                    }
                }
            }

            SetupThirdPersonForAllPlayers();
        }

        IEnumerator Transition(screenSetup_s newSetup)
        {
            float speed = 4.5f;

            if (Time.timeSinceLevelLoad > 1)
            {
                transitionImg.fillAmount = 0;
                transitionImg.color = new Color(1, 1, 1, 0);
            }

            while (transitionImg.fillAmount < 1 || transitionImg.color.a < 0.99f)
            {
                transitionImg.fillClockwise = true;
                transitionImg.fillAmount += Time.deltaTime * speed;
                transitionImg.color = Color.Lerp(transitionImg.color, Color.white, speed * 3 * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }

            //Disable all cameras 
            for (int i = 0; i < playerCameras.Length; i++)
            {
                playerCameras[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < newSetup.cameras; i++)
            {
                playerCameras[i].gameObject.SetActive(true);
                playerCameras[i].SetupCamera(newSetup.camInfos[i]);
            }

            if (Bam.MainHUDScript.singleton)
            {
                Bam.MainHUDScript.singleton.ToggleHUDLights(newSetup.cameras != 1);
            }

            for (float delay = 1; delay > 0; delay -= Time.deltaTime)
            {
                yield return new WaitForEndOfFrame();
            }

            while (transitionImg.fillAmount > 0)
            {
                transitionImg.fillClockwise = false;
                transitionImg.fillAmount -= Time.deltaTime * speed;

                yield return new WaitForEndOfFrame();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                screenSetup_s s = new screenSetup_s();
                s.cameras = 2;
                s.camInfos = new Bam.PlayerCameraScript.CameraInfo[4];
                s.camInfos[0].m_followThesePlayers = new bool[4] { true, false, false, false };
                s.camInfos[1].m_followThesePlayers = new bool[4] { false, true, true, true };

                s.camInfos[0].m_positionOnScreen = Bam.PlayerCameraScript.screenPositions_e.topHalf;
                s.camInfos[0].m_viewStyle = Bam.PlayerCameraScript.viewStyles_e.thirdPerson;

                s.camInfos[1].m_positionOnScreen = Bam.PlayerCameraScript.screenPositions_e.bottomHalf;
                s.camInfos[1].m_viewStyle = Bam.PlayerCameraScript.viewStyles_e.overhead;

                NewScreenSetup(s);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                screenSetup_s s = new screenSetup_s();
                s.cameras = 2;
                s.camInfos = new Bam.PlayerCameraScript.CameraInfo[4];
                s.camInfos[0].m_followThesePlayers = new bool[4] { true, false, false, false };
                s.camInfos[1].m_followThesePlayers = new bool[4] { false, true, false, false };

                s.camInfos[0].m_positionOnScreen = Bam.PlayerCameraScript.screenPositions_e.topHalf;
                s.camInfos[0].m_viewStyle = Bam.PlayerCameraScript.viewStyles_e.thirdPerson;

                s.camInfos[1].m_positionOnScreen = Bam.PlayerCameraScript.screenPositions_e.bottomHalf;
                s.camInfos[1].m_viewStyle = Bam.PlayerCameraScript.viewStyles_e.thirdPerson;

                NewScreenSetup(s);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
                SetupThirdPersonForAllPlayers();

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SetupSingularOverheadView(true, true, false, false);
            }
        }

        Bam.PlayerCameraScript[] CreateCameras(int number)
        {
            Bam.PlayerCameraScript[] newCams = new Bam.PlayerCameraScript[number];

            for (int i = 0; i < number; i++)
            {
                GameObject cam = Instantiate(playerCameraPrefab, transform.position, transform.rotation) as GameObject;
                newCams[i] = cam.GetComponent<Bam.PlayerCameraScript>();
            }

            return newCams;
        }

    }
}