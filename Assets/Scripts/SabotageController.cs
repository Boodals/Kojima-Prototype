using UnityEngine;
using System.Collections;
using System;

public class SabotageController : MonoBehaviour
{
    public int ChaseCar { get; set; }
    public static SabotageController Singleton { get { return m_singleton; } }
    private static SabotageController m_singleton;
   

    protected void Start()
    {
        if (Singleton != null)
            return;
        m_singleton = this;

        /*Set up the camera*/
        CameraManagerScript.ScreenSetup screenSetup = new CameraManagerScript.ScreenSetup(1);
        screenSetup.cameras = 1;
        screenSetup.camInfos = new PlayerCameraScript.CameraInfo[4];
        screenSetup.camInfos[0].followThesePlayers = new bool[4];
        for (int i = 0; i < GameController.currentPlayers; ++i) screenSetup.camInfos[0].followThesePlayers[i] = ChaseCar != i;
        screenSetup.camInfos[0].viewStyle = PlayerCameraScript.ViewStyles.Overhead;
        screenSetup.camInfos[0].positionOnScreen = PlayerCameraScript.ScreenPositions.FullScreen;
        CameraManagerScript.singleton.NewScreenSetup(screenSetup);


        /*Add off-screen death script to the ChaseCar*/
        GameController.singleton.players[ChaseCar].gameObject.AddComponent<KillIfOffscreen>();
    }



    protected void LateUpdate()
    {
     
    }
}
