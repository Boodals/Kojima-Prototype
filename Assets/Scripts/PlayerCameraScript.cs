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

        if(newViewStyle==ViewStyles.ThirdPerson && mainPlayer == null)
        {
            for(int i=0; i<followingThesePlayers.Length; i++)
            {
                if(followingThesePlayers[i])
                {
                    mainPlayer = GameController.singleton.players[i];
                    StartCoroutine("ResetThirdPersonAngle");
                    i = 99;
                }
            }
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

        Debug.Log("DONE");
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
        //if (freeCamTimer <= 0)
        //{
        //    Vector3 targetCurPos = mainPlayer.transform.eulerAngles;
        //    targetCurPos.y -= 180;
        //    curPos.y = Mathf.Lerp(curPos.y, Quaternion.Euler(targetCurPos).eulerAngles.y, 1 * Time.deltaTime);
        //}
        //else
        //    freeCamTimer -= Time.deltaTime;

        Vector3 backwards = new Vector3(0, 0, -(distanceFromPlayer));
        Quaternion rot = Quaternion.Euler(curPos);

        Vector3 targetPos = Vector3.zero;
        
        if(mainPlayer)
            targetPos = mainPlayer.transform.position + (rot * backwards);

        if (Input.GetKeyDown("joystick " + mainPlayer.playerIndex + " button 9"))
            StartCoroutine("ResetThirdPersonAngle");

        //Debug.DrawLine(mainPlayer.transform.position + Vector3.up, targetPos, Color.green);
        RaycastHit rH;
        if(Physics.Linecast(mainPlayer.transform.position + mainPlayer.transform.up, targetPos, out rH, LayerMask.GetMask("Default")))
        {
            //Debug.Log(rH.collider.gameObject.name);
            targetPos = rH.point + rH.normal*0.2f;
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

        targetFOV = 80 + distance * 0.35f;
        targetFOV = Mathf.Clamp(targetFOV, 80, 110);

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
