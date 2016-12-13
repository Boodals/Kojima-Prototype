using UnityEngine;
using System.Collections;

public class CarScript : MonoBehaviour
{
    public static bool playersCanMove = true;
    public bool canIMove = true;

    public int health = 100, playerIndex = 1;
    public float acceleration = 100, turnSpeed = 35;

    public Transform carBody;

    public float wheelSize = 4;
    [Tooltip("BL, BR, FL, FR")]
    public Transform[] wheels;
    public bool[] wheelIsGrounded;
    RaycastHit[] wheelRaycasts;
    public float wheelTorque;

    public float currentWheelSpeed = 0;

    Vector3[] wheelLocalPositions;

    Rigidbody rb;


    //Don't worry about these
    [HideInInspector]
    public string playerInputTag;

    float targetAngularDrag = 4;

    public float cancelHoriForce = 20;

    // Use this for initialization
    void Start()
    {
        //Physics.gravity = new Vector3(0, -60, 0);

        playerInputTag = "_P" + playerIndex;

        wheelLocalPositions = new Vector3[wheels.Length];
        wheelIsGrounded = new bool[wheels.Length];
        wheelRaycasts = new RaycastHit[wheels.Length];

        for (int i=0; i<wheelLocalPositions.Length; i++)
        {
            wheelLocalPositions[i] = wheels[i].transform.localPosition;
        }

        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float currentVelocity = rb.velocity.magnitude;
        currentWheelSpeed = currentVelocity * Vector3.Dot(rb.velocity.normalized, -transform.forward);

        rb.angularDrag = 0;

        if (playersCanMove && canIMove)
        {
            //Give each wheel a chance to push the car if grounded
            for (int i = 0; i < wheels.Length; i++)
            {
                wheelIsGrounded[i] = isWheelGrounded(wheels[i], i);
                

                if (wheelIsGrounded[i])
                {
                    rb.angularDrag += targetAngularDrag/4;

                    Vector3 direction = Vector3.Cross(wheelRaycasts[i].normal, transform.forward);
                    direction = Vector3.Cross(direction, transform.up);

                    Debug.DrawLine(transform.position, transform.position + direction, Color.green);
                    rb.AddForce(direction * acceleration * Input.GetAxisRaw("Acceleration" + playerInputTag), ForceMode.Acceleration);
                    rb.AddForce(-direction * acceleration * Input.GetAxisRaw("Brake" + playerInputTag), ForceMode.Acceleration);

                    rb.AddForce(-transform.up * currentWheelSpeed * 0.99525f);
                    PreventSkidding();
                }

                if(i>1)
                {
                    float curTorqueSpeed = turnSpeed * currentWheelSpeed;

                   // if (currentWheelSpeed < 2)
                        //curTorqueSpeed = 0;

                    curTorqueSpeed = Mathf.Clamp(curTorqueSpeed, -turnSpeed, turnSpeed);

                    if (wheelIsGrounded[i])
                    {
                        rb.AddTorque(transform.up * Input.GetAxisRaw("Horizontal" + playerInputTag) * (curTorqueSpeed), ForceMode.Acceleration);
                        //rb.transform.position -= transform.forward * rb.angularVelocity.magnitude/50;
                    }

                    float lerpValue = 0.5f + Input.GetAxis("Horizontal" + playerInputTag) * 0.5f;
                    float newY = Mathf.Lerp(-35, 35, lerpValue);                   

                    wheels[i].transform.localRotation = Quaternion.Lerp(wheels[i].transform.localRotation, Quaternion.Euler(new Vector3(wheels[i].localEulerAngles.x, newY, 0)), 8 * Time.deltaTime);
                }

                
            }
        }

        
    }

    void LateUpdate()
    {
        for(int i=0; i<wheels.Length; i++)
        {
            RotateWheel(wheels[i]);
        }
    }

    void RotateWheel(Transform wheel)
    {
        wheel.Rotate(Vector3.right * -currentWheelSpeed);
    }

    void PreventSkidding()
    {
        Vector3 velo = rb.velocity;
        Vector3 localVelo = transform.InverseTransformDirection(velo);

        float speedSimilarity = Mathf.Abs(Vector3.Dot(transform.forward, rb.velocity));


        wheelTorque = localVelo.z;
        localVelo.x = Mathf.Lerp(localVelo.x, 0, cancelHoriForce * (1) * Time.deltaTime);
        //localVelo.x *= 0.8f;

        rb.velocity = transform.TransformDirection(localVelo);
    }

    bool isWheelGrounded(Transform wheel, int index)
    {
        //wheelLocalPositions[index] = wheel.transform.local
        bool ret;

        ret = Physics.Raycast(wheel.transform.position, -transform.up, out wheelRaycasts[index], wheelSize, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);
        
        if(ret)
        {
            Debug.DrawLine(wheel.transform.position, wheelRaycasts[index].point, Color.red, 0.01f);
        }
        else
        {
            Debug.DrawLine(wheel.transform.position, wheel.transform.position -transform.up*wheelSize, Color.blue, 0.02f);
        }

        return ret;
    }
}
