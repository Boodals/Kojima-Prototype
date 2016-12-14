using UnityEngine;
using System.Collections;

public class PlayerCameraScript : MonoBehaviour {

    Camera cam;

    public enum ViewStyles { ThirdPerson, Overhead}
    public ViewStyles currentCameraType = ViewStyles.ThirdPerson;

    public CarScript[] myPlayers;

    float turnSpeed = 90;
    float distanceFromPlayer = 7.5f;


   //Overhead
    public Vector3 curPos;


    float targetFOV = 65;

	// Use this for initialization
	void Start () {
        curPos = -myPlayers[0].transform.eulerAngles;
        transform.position = myPlayers[0].transform.position;

        cam = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void ThirdPerson(Vector3 input)
    {
        targetFOV = 65;
        curPos += input * turnSpeed * Time.deltaTime;

        curPos.x = Mathf.Clamp(curPos.x, 10, 60);

        Vector3 backwards = new Vector3(0, 0, -distanceFromPlayer);
        Quaternion rot = Quaternion.Euler(curPos);

        transform.position = Vector3.Lerp(transform.position, myPlayers[0].transform.position + (rot * backwards), 8 * Time.deltaTime);

        transform.LookAt(myPlayers[0].transform.position);
    }

    Vector3 GetAveragePosition(CarScript[] players)
    {
        Vector3 pos = Vector3.zero;

        Vector3[] playerPos = new Vector3[players.Length];

        for(int i=0; i<playerPos.Length; i++)
        {
            playerPos[i] = players[i].transform.position;
        }

        pos = GetAveragePosition(playerPos);

        return pos;
    }

    Vector3 GetAveragePosition(Vector3[] positions)
    {
        Vector3 pos = Vector3.zero;

        float multiplier = 1 / (float)positions.Length;

        for(int i=0; i<positions.Length; i++)
        {
            positions[i] *= multiplier;
            pos += positions[i];
        }

        return pos;
    }

    void Overhead(Vector3 input)
    {
        Vector3 pos = GetAveragePosition(myPlayers);
        float height = 15;

        float distance = 0;

        for(int i=0; i<myPlayers.Length-1; i++)
        {
            if(i<myPlayers.Length)
                distance += Vector3.Distance(myPlayers[i].transform.position, myPlayers[i + 1].transform.position);
        }

        targetFOV = 80 + distance * 0.5f;
        targetFOV = Mathf.Clamp(targetFOV, 80, 110);

        height += distance * 0.2f;

        pos += Vector3.up * height;

        transform.position = Vector3.Lerp(transform.position, pos, 8 * Time.deltaTime);

        transform.rotation = Quaternion.LookRotation(-Vector3.up);
    }

    void FixedUpdate()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Vertical2" + myPlayers[0].playerInputTag), Input.GetAxisRaw("Horizontal2" + myPlayers[0].playerInputTag), 0);

        switch(currentCameraType)
        {
            case ViewStyles.ThirdPerson:
                ThirdPerson(input);
                break;
            case ViewStyles.Overhead:
                Overhead(input);
                break;
        }

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, 4 * Time.deltaTime);
    }
}
