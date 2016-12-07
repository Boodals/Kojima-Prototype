using UnityEngine;
using System.Collections;

public class PlayerCameraScript : MonoBehaviour {

    public PlayerScript myPlayer;

    float turnSpeed = 8;
    float distanceFromPlayer = 4.5f;

    
    public Vector3 curPos;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void FixedUpdate()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw(myPlayer.horizontal2), Input.GetAxisRaw(myPlayer.vertical2), 0);

        curPos += input * turnSpeed * Time.deltaTime;

        curPos.y = Mathf.Clamp(curPos.y, -60, 60);


        transform.position = myPlayer.transform.position + curPos;
    }
}
