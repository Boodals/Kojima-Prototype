using UnityEngine;
using System.Collections;

//This is basically all debug stuff right now
namespace Kojima
{
    public class GameController : MonoBehaviour
    {
        public static GameController s_singleton;

        public static int s_ncurrentPlayers = 0;
        public CarScript[] m_players;

        //Managers and HUD stuff etc should be added to this
        public GameObject[] createTheseOnAwake;

        // Use this for initialization
        void Awake()
        {
            if (!s_singleton)
            {
                s_singleton = this;
                m_players = new CarScript[4];

                for (int i = 0; i < createTheseOnAwake.Length; i++)
                {
                    GameObject newObj = Instantiate<GameObject>(createTheseOnAwake[i]);
                    DontDestroyOnLoad(newObj);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
        private void Start()
        {
            //CameraManagerScript.singleton.SetupThirdPersonForAllPlayers();
        }
        // Update is called once per frame
        void Update()
        {

            if (Input.GetKeyDown(KeyCode.Alpha4))
                CameraManagerScript.singleton.SetupLobbyCamera();

            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                CameraManagerScript.screenSetup_s newScreenSetup = new CameraManagerScript.screenSetup_s(4);
                newScreenSetup.camInfos[0].m_nfollowThisPlayer = 1;
                newScreenSetup.camInfos[1].m_nfollowThisPlayer = 2;
                newScreenSetup.camInfos[2].m_nfollowThisPlayer = 3;
                newScreenSetup.camInfos[3].m_nfollowThisPlayer = 4;

                newScreenSetup.camInfos[0].m_positionOnScreen = Bam.PlayerCameraScript.screenPositions_e.topLeft;
                newScreenSetup.camInfos[1].m_positionOnScreen = Bam.PlayerCameraScript.screenPositions_e.topRight;
                newScreenSetup.camInfos[2].m_positionOnScreen = Bam.PlayerCameraScript.screenPositions_e.bottomLeft;
                newScreenSetup.camInfos[3].m_positionOnScreen = Bam.PlayerCameraScript.screenPositions_e.bottomRight;

                newScreenSetup.camInfos[0].m_viewStyle = Bam.PlayerCameraScript.viewStyles_e.overhead;
                newScreenSetup.camInfos[1].m_viewStyle = Bam.PlayerCameraScript.viewStyles_e.overhead;
                newScreenSetup.camInfos[2].m_viewStyle = Bam.PlayerCameraScript.viewStyles_e.overhead;
                newScreenSetup.camInfos[3].m_viewStyle = Bam.PlayerCameraScript.viewStyles_e.overhead;

                CameraManagerScript.singleton.NewScreenSetup(newScreenSetup);
            }
        }

        public void AllCarsCanMove(bool _b)
        {
            foreach (var car in m_players)
            {
                car.CanMove = _b;
            }
        }
    }
}