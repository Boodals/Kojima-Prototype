using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

    public static GameController singleton;

    public static int currentPlayers = 0;
    public CarScript[] players;

	// Use this for initialization
	void Awake () {
        singleton = this;
        players = new CarScript[4];
        
	}
    private void Start()
    {
        //CameraManagerScript.singleton.SetupThirdPersonForAllPlayers();
    }
    // Update is called once per frame
    void Update () {

        if (Input.GetKeyDown(KeyCode.Alpha4))
            CameraManagerScript.singleton.SetupLobbyCamera();

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            CameraManagerScript.ScreenSetup newScreenSetup = new CameraManagerScript.ScreenSetup(4);
            newScreenSetup.camInfos[0].followThisPlayer = 1;
            newScreenSetup.camInfos[1].followThisPlayer = 2;
            newScreenSetup.camInfos[2].followThisPlayer = 3;
            newScreenSetup.camInfos[3].followThisPlayer = 4;

            newScreenSetup.camInfos[0].positionOnScreen = PlayerCameraScript.ScreenPositions.TopLeft;
            newScreenSetup.camInfos[1].positionOnScreen = PlayerCameraScript.ScreenPositions.TopRight;
            newScreenSetup.camInfos[2].positionOnScreen = PlayerCameraScript.ScreenPositions.BottomLeft;
            newScreenSetup.camInfos[3].positionOnScreen = PlayerCameraScript.ScreenPositions.BottomRight;

            newScreenSetup.camInfos[0].viewStyle = PlayerCameraScript.ViewStyles.Overhead;
            newScreenSetup.camInfos[1].viewStyle = PlayerCameraScript.ViewStyles.Overhead;
            newScreenSetup.camInfos[2].viewStyle = PlayerCameraScript.ViewStyles.Overhead;
            newScreenSetup.camInfos[3].viewStyle = PlayerCameraScript.ViewStyles.Overhead;

            CameraManagerScript.singleton.NewScreenSetup(newScreenSetup);
        }
	}

    public void AllCarsCanMove(bool _b)
    {
        foreach (var car in players)
        {
            car.CanMove = _b;
        }
    }
}
