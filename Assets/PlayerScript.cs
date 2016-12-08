using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {

    public WheelCollider[] wheels;
    public Transform[] visualWheels;

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

        //Rotate front wheels
        for (int i = 2; i < 4; i++)
        {
            wheels[i].steerAngle = Mathf.Lerp(wheels[i].steerAngle, 45 * input.x, 4 * Time.deltaTime);
        }

        //Accelerate using back wheels
        for (int i = 0; i < 2; i++)
        {
            wheels[i].motorTorque = 1000 * Input.GetAxisRaw(accelerate);
        }

        for (int i=0; i<wheels.Length; i++)
        {
            Vector3 pos;
            Quaternion rot;

            wheels[i].GetWorldPose(out pos, out rot);

            visualWheels[i].transform.position = pos;
            visualWheels[i].transform.rotation = rot;
        }
    }
}
