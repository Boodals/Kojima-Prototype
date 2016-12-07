using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {

    public Rigidbody[] wheels;

    public string horizontal, vertical, horizontal2, vertical2, accelerate, brake, drift, handbrake;

	// Use this for initialization
	void Start () {
	
	}

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw(horizontal), Input.GetAxisRaw(vertical), 0);
    }
}
