using UnityEngine;
using System.Collections;

public class PlayerCameraScript : MonoBehaviour {

    public PlayerScript myPlayer;

    float turnSpeed = 90;
    float distanceFromPlayer = 7.5f;
   
    public Vector3 curPos;

	// Use this for initialization
	void Start () {
        curPos = -myPlayer.transform.eulerAngles;
        transform.position = myPlayer.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void FixedUpdate()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw(myPlayer.vertical2), Input.GetAxisRaw(myPlayer.horizontal2), 0);

        curPos += input * turnSpeed * Time.deltaTime;

        curPos.x = Mathf.Clamp(curPos.x, 10, 60);

        Vector3 backwards = new Vector3(0, 0, -distanceFromPlayer);
        Quaternion rot = Quaternion.Euler(curPos);

        transform.position = Vector3.Lerp(transform.position, myPlayer.transform.position + (rot * backwards), 8 * Time.deltaTime);

        transform.LookAt(myPlayer.transform.position);
    }
}
