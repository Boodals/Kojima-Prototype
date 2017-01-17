using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CameraManagerScript : MonoBehaviour {

    //These are for convenience when you set up a new view
    public static bool[] FollowPlayerOne, FollowPlayerTwo, FollowPlayerThree, FollowPlayerFour, FollowAll;

    public static CameraManagerScript singleton;

    [System.Serializable]
    public struct ScreenSetup
    {
        public int cameras;
        public PlayerCameraScript.CameraInfo[] camInfos;

        public ScreenSetup(int howManyCameras)
        {
            cameras = howManyCameras;
            camInfos = new PlayerCameraScript.CameraInfo[cameras];

            for(int i=0; i<camInfos.Length; i++)
            {
                camInfos[i].followThesePlayers = new bool[4];
            }
        }
    }

    public GameObject playerCameraPrefab;
    public PlayerCameraScript[] playerCameras;

    public Image transitionImg;

    //QUICK SETUPS
    public void SetupOverhead()
    {
        ScreenSetup newSS = new ScreenSetup();
        newSS.cameras = 1;
        newSS.camInfos = new PlayerCameraScript.CameraInfo[4];

        newSS.camInfos[0].followThesePlayers = new bool[4] { true, true, true, true };
        newSS.camInfos[0].viewStyle = PlayerCameraScript.ViewStyles.Overhead;
        newSS.camInfos[0].positionOnScreen = PlayerCameraScript.ScreenPositions.FullScreen;

        NewScreenSetup(newSS);
    }

    public void SetupThirdPersonForAllPlayers()
    {
        ScreenSetup newSS = new ScreenSetup();
        newSS.cameras = GameController.currentPlayers;

        newSS.camInfos = new PlayerCameraScript.CameraInfo[4];

        for (int i = 0; i < 4; i++)
        {
            newSS.camInfos[i].followThesePlayers = new bool[4];

            for (int fTP = 0; fTP < 4; fTP++)
            {
                newSS.camInfos[i].followThesePlayers[fTP] = fTP == i;
            }

            newSS.camInfos[i].positionOnScreen = (PlayerCameraScript.ScreenPositions)i;
        }

        if (GameController.currentPlayers == 2)
        {
            newSS.camInfos[0].positionOnScreen = PlayerCameraScript.ScreenPositions.TopHalf;
            newSS.camInfos[1].positionOnScreen = PlayerCameraScript.ScreenPositions.BottomHalf;
        }

        if (GameController.currentPlayers == 1)
        {
            newSS.camInfos[0].positionOnScreen = PlayerCameraScript.ScreenPositions.FullScreen;
        }

        NewScreenSetup(newSS);
    }

    //OTHER PUBLIC FUNCTIONS
    public void NewScreenSetup(ScreenSetup newSetup)
    { 
        StopCoroutine("Transition");
        StartCoroutine("Transition", newSetup);
    }


    void Awake()
    {
        singleton = this;
    }

	// Use this for initialization
	void Start () {
        playerCameras = CreateCameras(GameController.currentPlayers);
        //SetupThirdPersonForAllPlayers();

        FollowPlayerOne = new bool[4] { true, false, false, false };
        FollowPlayerTwo = new bool[4] { false, true, false, false };
        FollowPlayerThree = new bool[4] { false, false, true, false };
        FollowPlayerFour = new bool[4] { false, false, false, true };
        FollowAll = new bool[4] { true, true, true, true };

        transitionImg.fillAmount = 1;
        transitionImg.color = Color.white;

        for(int i=0; i<4; i++)
        {
            MainHUDScript.singleton.playerHUDs[i].GetComponent<Canvas>().worldCamera = playerCameras[i].GetUICameraComponent();
            MainHUDScript.singleton.playerHUDs[i].GetComponent<Canvas>().planeDistance = 0.5f;
        }
    }

    IEnumerator Transition(ScreenSetup newSetup)
    {
        float speed = 2.5f;

        transitionImg.fillAmount = 0;
        transitionImg.color = new Color(1, 1, 1, 0);

        while(transitionImg.fillAmount<1 || transitionImg.color.a<0.99f)
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

        MainHUDScript.singleton.ToggleHUDLights(newSetup.cameras!=1);

        for (float delay = 1; delay>0; delay-=Time.deltaTime)
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
	void Update () {
	if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            ScreenSetup s = new ScreenSetup();
            s.cameras = 2;
            s.camInfos = new PlayerCameraScript.CameraInfo[4];
            s.camInfos[0].followThesePlayers = new bool[4] { true, false, false, false };
            s.camInfos[1].followThesePlayers = new bool[4] { false, true, true, true };

            s.camInfos[0].positionOnScreen = PlayerCameraScript.ScreenPositions.TopHalf;
            s.camInfos[0].viewStyle = PlayerCameraScript.ViewStyles.ThirdPerson;

            s.camInfos[1].positionOnScreen = PlayerCameraScript.ScreenPositions.BottomHalf;
            s.camInfos[1].viewStyle = PlayerCameraScript.ViewStyles.Overhead;

            NewScreenSetup(s);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ScreenSetup s = new ScreenSetup();
            s.cameras = 2;
            s.camInfos = new PlayerCameraScript.CameraInfo[4];
            s.camInfos[0].followThesePlayers = new bool[4] { true, false, false, false };
            s.camInfos[1].followThesePlayers = new bool[4] { false, true, false, false };

            s.camInfos[0].positionOnScreen = PlayerCameraScript.ScreenPositions.TopHalf;
            s.camInfos[0].viewStyle = PlayerCameraScript.ViewStyles.ThirdPerson;

            s.camInfos[1].positionOnScreen = PlayerCameraScript.ScreenPositions.BottomHalf;
            s.camInfos[1].viewStyle = PlayerCameraScript.ViewStyles.ThirdPerson;

            NewScreenSetup(s);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetupThirdPersonForAllPlayers();
    }

    PlayerCameraScript[] CreateCameras(int number)
    {
        PlayerCameraScript[] newCams = new PlayerCameraScript[number];

        for(int i=0; i<number; i++)
        {
            GameObject cam = Instantiate(playerCameraPrefab, transform.position, transform.rotation) as GameObject;
            newCams[i] = cam.GetComponent<PlayerCameraScript>();
        }

        return newCams;
    }

}
