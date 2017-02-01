//Author:       Yams
//Description:  Script to help set up a game of Sabotage.
//Last Edit:    Yams @ 15/01/2017

using UnityEngine;
using System.Collections;
using System;

namespace Bam
{
    public class SabotageController : MonoBehaviour
    {
        public int ChaseCar { get; set; }
        public static SabotageController Singleton { get { return m_singleton; } }
        private static SabotageController m_singleton;
        //public GameObject m_projectorObject;
        public bool m_firstFrame = true;

        protected void Start()
        {
            if (Singleton != null)
                return;
            m_singleton = this;

            /*Set up the camera*/
            Kojima.CameraManagerScript.screenSetup_s screenSetup = new Kojima.CameraManagerScript.screenSetup_s(1);
            screenSetup.cameras = 1;
            screenSetup.camInfos = new PlayerCameraScript.CameraInfo[4];
            screenSetup.camInfos[0].m_followThesePlayers = new bool[4];
            for (int i = 0; i < Kojima.GameController.s_ncurrentPlayers; ++i) screenSetup.camInfos[0].m_followThesePlayers[i] = ChaseCar != i;
            screenSetup.camInfos[0].m_viewStyle = PlayerCameraScript.viewStyles_e.overhead;
            screenSetup.camInfos[0].m_positionOnScreen = PlayerCameraScript.screenPositions_e.fullScreen;
            Kojima.CameraManagerScript.singleton.NewScreenSetup(screenSetup);
        }

        protected void Update()
        {
            /*Hacky work around setup ordering*/
            if (m_firstFrame)
            {
                //Camera ref_mainCam = CameraManagerScript.singleton.playerCameras[0].Cam;
                //m_projectorObject.transform.SetParent(ref_mainCam.transform);
                ///*Attach the projector to the overhead camera*/
                //m_projectorObject.transform.localRotation = Quaternion.identity;
                //m_projectorObject.transform.localPosition = Vector3.zero;


                /*Add off-screen death script to the ChaseCar*/
                Kojima.GameController.s_singleton.m_players[ChaseCar].gameObject.AddComponent<KillIfOffscreen>();
            }
        }
    }
}