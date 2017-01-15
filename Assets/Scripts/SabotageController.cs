//Author:       Yams
//Description:  Script to help set up a game of Sabotage.
//Last Edit:    Yams @ 15/01/2017

using UnityEngine;
using System.Collections;
using System;

public class SabotageController : MonoBehaviour
{
    public int ChaseCar { get; set; }
    public static SabotageController Singleton { get { return m_singleton; } }
    private static SabotageController m_singleton;
    public GameObject m_projectorObject;
    public bool m_firstFrame = true;

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
        
    }

    protected void Update()
    {
        /*Hacky work around setup ordering*/
        if (m_firstFrame)
        {
            Camera ref_mainCam = CameraManagerScript.singleton.playerCameras[0].Cam;
            m_projectorObject.transform.SetParent(ref_mainCam.transform);
            /*Attach the projector to the overhead camera*/
            m_projectorObject.transform.localRotation = Quaternion.identity;
            m_projectorObject.transform.localPosition = Vector3.zero;


            /*Add off-screen death script to the ChaseCar*/
            GameController.singleton.players[ChaseCar].gameObject.AddComponent<KillIfOffscreen>();
        }
    }
}
