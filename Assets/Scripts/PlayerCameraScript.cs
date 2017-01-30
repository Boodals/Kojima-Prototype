﻿//Author:       TMS
//Description:  Script that handles a player's camera.
//Last Edit:    Yams @ 14/01/2017  

using UnityEngine;
using System.Collections;

public class PlayerCameraScript : MonoBehaviour {

    [System.Serializable]
    public struct CameraInfo
    {
        public ViewStyles viewStyle;
        public ScreenPositions positionOnScreen;

        public int followThisPlayer;
        public bool[] followThesePlayers;
    }

    [System.Serializable]
    public enum ScreenPositions { TopLeft, TopRight, BottomLeft, BottomRight, TopHalf, BottomHalf, FullScreen }

    public Camera Cam{ get { return cam; } }
    Camera cam, UICam;

    public enum ViewStyles { ThirdPerson, Overhead}
    public ViewStyles currentViewStyle = ViewStyles.ThirdPerson;

    //public CarScript[] myPlayers;
    public CarScript mainPlayer;
    public bool[] followingThesePlayers;

    float turnSpeed = 90;
    float distanceFromPlayer = 7.5f;




   //Overhead
    public Vector3 curPos;

    //Third person
    [SerializeField]
    float freeCamTimer = 0;


    float targetFOV = 65;

	// Use this for initialization
	void Awake () {

        followingThesePlayers = new bool[4];
        cam = GetComponent<Camera>();
        UICam = transform.FindChild("UICamera").GetComponent<Camera>();
	}

    void Start()
    {
        if (mainPlayer)
        {
            curPos = -mainPlayer.transform.eulerAngles;
            transform.position = mainPlayer.transform.position;
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public Camera GetCameraComponent()
    {
        return cam;
    }

    public Camera GetUICameraComponent()
    {
        return UICam;
    }

    public void SetupCamera(CameraInfo newInfo)
    {
        mainPlayer = null;

        if (newInfo.followThisPlayer > 0)
            mainPlayer = GameController.singleton.players[newInfo.followThisPlayer - 1];

        followingThesePlayers = newInfo.followThesePlayers;
        SwitchViewStyle(newInfo.viewStyle);

        switch(newInfo.positionOnScreen)
        {
            case ScreenPositions.BottomLeft:
                MoveScreenToHere(new Vector2(0, 0), new Vector2(0.5f, 0.5f));
                break;
            case ScreenPositions.BottomRight:
                MoveScreenToHere(new Vector2(0.5f, 0), new Vector2(0.5f, 0.5f));
                break;
            case ScreenPositions.TopLeft:
                MoveScreenToHere(new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f));
                break;
            case ScreenPositions.TopRight:
                MoveScreenToHere(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                break;

            case ScreenPositions.TopHalf:
                MoveScreenToHere(new Vector2(0.0f, 0.5f), new Vector2(1f, 0.5f));
                break;
            case ScreenPositions.BottomHalf:
                MoveScreenToHere(new Vector2(0.0f, 0.0f), new Vector2(1f, 0.5f));
                break;

            case ScreenPositions.FullScreen:
                MoveScreenToHere(new Vector2(0.0f, 0.0f), new Vector2(1f, 1f));
                break;
        }
    }

    public void SwitchViewStyle(ViewStyles newViewStyle)
    {
        currentViewStyle = newViewStyle;

        switch (newViewStyle)
        {
            case ViewStyles.Overhead:
                cam.orthographic = true;
                break;
            case ViewStyles.ThirdPerson:

                cam.orthographic = false;
                if(mainPlayer==null)
                {
                    for (int i = 0; i < followingThesePlayers.Length; i++)
                    {
                        if (followingThesePlayers[i])
                        {
                            mainPlayer = GameController.singleton.players[i];
                            StartCoroutine("ResetThirdPersonAngle");
                            i = 99;
                        }
                    }
                }
                break;
        }
    }

    IEnumerator ResetThirdPersonAngle()
    {
        float targetY = mainPlayer.transform.eulerAngles.y - 180;
        float amount = Mathf.Abs(targetY - curPos.y);
        float speed = 5;

        for(float y = 0; y<amount-0.05f; y+=Time.deltaTime)
        {
            curPos.x = Mathf.Lerp(curPos.x, 0, speed * Time.deltaTime);
            curPos.y = Mathf.Lerp(curPos.y, targetY, speed * Time.deltaTime);
            y = Mathf.Lerp(y, amount, speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        //Debug.Log("DONE");
    }

    void ThirdPerson(Vector3 input)
    {
        if (!mainPlayer)
            return;

        targetFOV = 60 + mainPlayer.currentWheelSpeed * 0.15f;
        curPos += input * turnSpeed * Time.deltaTime;
        curPos.x = Mathf.Clamp(curPos.x, 10, 60);

        if (input.magnitude > 0.1f)
            freeCamTimer = 4;

        //Handles the "automatic" third person camera
        if (freeCamTimer <= 0 && !mainPlayer.InMidAir)
        {
            Vector3 targetCurPos = mainPlayer.transform.eulerAngles;
            targetCurPos.y -= 180;
            curPos.y = Mathf.LerpAngle(curPos.y, Quaternion.Euler(targetCurPos).eulerAngles.y, 2 * Time.deltaTime);
            curPos.x = Mathf.LerpAngle(curPos.x, 25, 1 * Time.deltaTime);
        }
        else
        {
            freeCamTimer -= Time.deltaTime;
        }

        float distanceMultiplier = 1;

        distanceMultiplier += Mathf.Abs(mainPlayer.transform.up.x * 2);

        Vector3 backwards = new Vector3(0, 0, -(distanceFromPlayer * distanceMultiplier));
        Quaternion rot = Quaternion.Euler(curPos);

        Vector3 targetPos = Vector3.zero;
        
        if(mainPlayer)
            targetPos = mainPlayer.transform.position + (rot * backwards);

        if (Input.GetKeyDown("joystick " + mainPlayer.playerIndex + " button 9"))
            StartCoroutine("ResetThirdPersonAngle");

        //Debug.DrawLine(mainPlayer.transform.position + Vector3.up, targetPos, Color.green);
        RaycastHit rH;
        if(Physics.Linecast(mainPlayer.transform.position + Vector3.up, targetPos, out rH, LayerMask.GetMask("Default")))
        {
            //Debug.Log(rH.collider.gameObject.name);
            //Debug.Log(mainPlayer.transform.forward);

            //if (Mathf.Abs(mainPlayer.transform.forward.y) > 0.45f)
                curPos.x += mainPlayer.transform.forward.y * Time.deltaTime * 10;

            //Debug.Log(Vector3.Distance(transform.position, mainPlayer.transform.position));

            if (Vector3.Distance(transform.position, mainPlayer.transform.position) < 3)
                curPos.x += 15 * Time.deltaTime;

            

            targetPos = rH.point + rH.normal*0.25f;
        }

        transform.position = Vector3.Lerp(transform.position, targetPos + mainPlayer.GetVelocity() * Time.deltaTime, 8 * Time.deltaTime);
        transform.LookAt(mainPlayer.transform.position);
    }

    Vector3 GetAveragePosition(CarScript[] players)
    {
        Vector3 pos = Vector3.zero;

        Vector3[] playerPos = new Vector3[players.Length];
        int playersToFollow = 0;

        for(int i=0; i<playerPos.Length; i++)
        {
            if (players[i] && followingThesePlayers[i])
            {
                playerPos[i] = players[i].transform.position;
                playersToFollow++;
            }
        }

        pos = GetAveragePosition(playerPos, playersToFollow);

        return pos;
    }

    Vector3 GetAveragePosition(Vector3[] positions, float divide = 0)
    {
        Vector3 pos = Vector3.zero;

        if (divide == 0)
            divide = positions.Length;

        float multiplier = 1 / (float)divide;

        for(int i=0; i<positions.Length; i++)
        {
            positions[i] *= multiplier;
            pos += positions[i];
        }

        return pos;
    }

    void Overhead(Vector3 input)
    {
        bool orthographicMode = true;

        Vector3 pos = Vector3.zero;

        if (!mainPlayer)
            pos = GetAveragePosition(GameController.singleton.players);
        else
            pos = mainPlayer.transform.position;

        float height = 15;
        float distance = 0;

        for(int i=0; i< GameController.singleton.players.Length-1; i++)
        {
            if (i < GameController.singleton.players.Length)
            {
                if (GameController.singleton.players[i] && GameController.singleton.players[i+1] && followingThesePlayers[i])
                {
                    distance += Vector3.Distance(GameController.singleton.players[i].transform.position, GameController.singleton.players[i + 1].transform.position);
                }
            }
        }

        targetFOV = 100 + distance * 0.35f;
        targetFOV = Mathf.Clamp(targetFOV, 80, 120);

        if (orthographicMode)
        {
            //cam.orthographic = true;
            cam.orthographicSize = 10 + distance * 0.5f;
            distance = 100;
        }
        else
        {
            //cam.orthographic = false;
        }

        height += distance * 0.1f;

        pos += Vector3.up * height;

        transform.position = Vector3.Lerp(transform.position, pos, 8 * Time.deltaTime);

        transform.rotation = Quaternion.LookRotation(-Vector3.up);
    }

    void FixedUpdate()
    {
        Vector3 input = Vector3.zero;
        
        if(mainPlayer)
            input = new Vector3(-Input.GetAxisRaw("Vertical2" + mainPlayer.playerInputTag), Input.GetAxisRaw("Horizontal2" + mainPlayer.playerInputTag), 0);

        switch(currentViewStyle)
        {
            case ViewStyles.ThirdPerson:
                ThirdPerson(input);
                break;
            case ViewStyles.Overhead:
                Overhead(input);
                break;
        }

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, 4 * Time.deltaTime);
        UICam.fieldOfView = cam.fieldOfView;
    }

    public void MoveScreenToHere(Vector2 _newPos, Vector2 _size)
    {
        Rect newRect = cam.rect;
        newRect.position = _newPos;
        newRect.size = _size;

        cam.rect = newRect;
        UICam.rect = cam.rect;
    }
}
