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

    Vector3 bodyVelocity, bodyAngularVelocity;

    float flipTimer = 0;

    //Don't worry about these
    [HideInInspector]
    public string playerInputTag;

    float targetAngularDrag = 4;

    public float cancelHoriForce = 20;

    // Use this for initialization
    void Start()
    {
        playerInputTag = "_P" + playerIndex;

        wheelLocalPositions = new Vector3[wheels.Length];
        wheelIsGrounded = new bool[wheels.Length];
        wheelRaycasts = new RaycastHit[wheels.Length];

        for (int i=0; i<wheelLocalPositions.Length; i++)
        {
            wheelLocalPositions[i] = wheels[i].transform.localPosition;
        }

        rb = GetComponent<Rigidbody>();

        bodyVelocity = Vector3.zero;
        bodyAngularVelocity = Vector3.zero;
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
                

                if (wheelIsGrounded[i] && i<=1)
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

                    curTorqueSpeed = Mathf.Clamp(curTorqueSpeed, -turnSpeed, turnSpeed);

                    if (wheelIsGrounded[i])
                    {
                        rb.AddTorque(transform.up * Input.GetAxisRaw("Horizontal" + playerInputTag) * (curTorqueSpeed), ForceMode.Acceleration);
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

        //Suspension
        carBody.transform.position += Vector3.up * -rb.velocity.y * 0.1f;

        carBody.transform.localPosition = Vector3.ClampMagnitude(carBody.transform.localPosition * 0.01f, 0.1f);
        carBody.transform.localPosition = Vector3.Lerp(carBody.transform.localPosition, Vector3.zero, 1 * Time.deltaTime);
        carBody.transform.localEulerAngles = Vector3.Lerp(carBody.transform.localEulerAngles, Vector3.zero, 3 * Time.deltaTime);
    }



    IEnumerator RollBackOver()
    {
        while(transform.up.y<0.995f)
        {
            //rb.useGravity = false;
            //rb.isKinematic = true;
            //Debug.Log("ROLLIN");
            rb.rotation = Quaternion.Lerp(rb.rotation, Quaternion.LookRotation(transform.forward, Vector3.up), 5 * Time.deltaTime);
            Debug.DrawLine(transform.position, transform.position + Vector3.up, Color.red);
            //rb.AddForce(Vector3.up * 11, ForceMode.Acceleration);
            yield return new WaitForSeconds(0.01f);
        }

        rb.useGravity = true;
        //rb.isKinematic = false;
    }

    void OnCollisionStay()
    {
        if (!wheelIsGrounded[0] && !wheelIsGrounded[1] && rb.velocity.magnitude<5)
        {
            flipTimer += Time.deltaTime;

            if(flipTimer>3)
                StartCoroutine("RollBackOver");
        }
        else
        {
            flipTimer = 0;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        carBody.transform.position += rb.velocity * 0.01f;
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

        rb.velocity = transform.TransformDirection(localVelo);
    }

    bool isWheelGrounded(Transform wheel, int index)
    {
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
