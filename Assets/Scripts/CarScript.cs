using UnityEngine;
using System.Collections;

public class CarScript : MonoBehaviour
{
    [System.Serializable]
    public struct CarInfo
    {
        public enum DriveMode { RearWheels, FrontWheels, AllWheels};
        public DriveMode myDriveMode;

        public float health, acceleration, turnSpeed, wheelSize;

        public CarInfo(float _health, float _acceleration, float _turnSpeed, float _wheelSize, DriveMode _driveMode)
        {
            myDriveMode = _driveMode;
            health = _health;
            acceleration = _acceleration;
            turnSpeed = _turnSpeed;
            wheelSize = _wheelSize;
        }
    }

    public static bool playersCanMove = true;
    public bool canIMove = true;

    public int playerIndex = 1;

    public CarInfo myInfo;

    public Transform carBody;

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

    [SerializeField]
    bool drifting = false;
    Vector3 driftVelo;

    float targetAngularDrag = 5;

    public float cancelHoriForce = 20;

    // Use this for initialization
    void Awake()
    {
        playerInputTag = "_P" + 1;

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

        GameController.currentPlayers++;
        GameController.singleton.players[playerIndex-1] = this;
    }

    void Start()
    {
        ApplyCarInfo(new CarInfo(100, 9, 12, 0.65f, CarInfo.DriveMode.AllWheels));
    }

    void OnDestroy()
    {
        GameController.currentPlayers--;
    }

    public void ApplyCarInfo(CarInfo newInfo)
    {
        myInfo = newInfo;
    }

    public Vector3 GetVelocity()
    {
        return rb.velocity;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float currentVelocity = rb.velocity.magnitude;
        currentWheelSpeed = currentVelocity * Vector3.Dot(rb.velocity.normalized, -transform.forward);

        rb.angularDrag = 0;

        if (playersCanMove && canIMove)
        {
            //Drifting
            drifting = Input.GetKey("joystick " + playerIndex + " button 1");

            if (drifting)
                cancelHoriForce = 0;
            else
                cancelHoriForce = Mathf.Lerp(cancelHoriForce, 3, 3 * Time.deltaTime);

            //Give each wheel a chance to push the car if grounded
            for (int i = 0; i < wheels.Length; i++)
            {
                wheelIsGrounded[i] = isWheelGrounded(wheels[i], i);

                bool thisWheelCanDrive = false;

                switch(myInfo.myDriveMode)
                {
                    case CarInfo.DriveMode.AllWheels:
                        thisWheelCanDrive = true;
                        break;
                    case CarInfo.DriveMode.RearWheels:
                        if(i<=1)
                            thisWheelCanDrive = true;
                        break;
                    case CarInfo.DriveMode.FrontWheels:
                        if(i>1)
                            thisWheelCanDrive = true;
                        break;
                }

                if (!wheelIsGrounded[i])
                    thisWheelCanDrive = false;
                else
                    PreventSkidding();

                if (thisWheelCanDrive)
                {
                    rb.angularDrag += targetAngularDrag/4;

                    float forwardsMultiplier = Input.GetAxisRaw("Acceleration" + playerInputTag) + (-Input.GetAxisRaw("Brake" + playerInputTag));

                    if (forwardsMultiplier != 0)
                    {
                        //Debug.Log(forwardsMultiplier);
                        Vector3 direction = Vector3.Cross(wheelRaycasts[i].normal, transform.forward);
                        direction = Vector3.Cross(direction, wheelRaycasts[i].normal);

                        Debug.DrawLine(wheels[i].transform.position, wheels[i].transform.position + direction * 5, Color.green);
                        rb.AddForce(direction * myInfo.acceleration * Input.GetAxisRaw("Acceleration" + playerInputTag), ForceMode.Acceleration);
                        rb.AddForce(-direction * myInfo.acceleration * Input.GetAxisRaw("Brake" + playerInputTag) * 0.55f, ForceMode.Acceleration);

                        rb.AddForce(-wheelRaycasts[i].normal * currentWheelSpeed * 2);


                        rb.rotation = Quaternion.RotateTowards(rb.rotation, Quaternion.LookRotation(direction, wheelRaycasts[i].normal), 35 * Time.deltaTime);
                    }
                    //carBody.transform.localEulerAngles += Vector3.right * currentWheelSpeed * 0.1f;
                }

                if(i>1)
                {
                    float curTorqueSpeed = myInfo.turnSpeed * currentWheelSpeed;

                    curTorqueSpeed = Mathf.Clamp(curTorqueSpeed, -myInfo.turnSpeed, myInfo.turnSpeed);

                    if (wheelIsGrounded[i])
                    {
                        rb.AddTorque(transform.up * Input.GetAxisRaw("Horizontal" + playerInputTag) * (curTorqueSpeed), ForceMode.Acceleration);
                    }
                }

                
            }
        }        
    }

    void LateUpdate()
    {
        for(int i=0; i<wheels.Length; i++)
        {
            RotateWheel(wheels[i]);

            if (i > 1)
            {
                float lerpValue = 0.5f + Input.GetAxis("Horizontal" + playerInputTag) * 0.5f;
                float newY = Mathf.Lerp(-35, 35, lerpValue);

                wheels[i].transform.localRotation = Quaternion.Lerp(wheels[i].transform.localRotation, Quaternion.Euler(new Vector3(wheels[i].localEulerAngles.x, newY, 0)), 8 * Time.deltaTime);
            }
        }

        //Suspension
        carBody.transform.position += Vector3.up * -rb.velocity.y * 0.1f;

        carBody.transform.localPosition = Vector3.ClampMagnitude(carBody.transform.localPosition * 0.01f, 0.15f);
        carBody.transform.localPosition = Vector3.Lerp(carBody.transform.localPosition, Vector3.zero, 1 * Time.deltaTime);

        Vector3 targetEuler = Vector3.zero;
        Vector3 veloLocal = transform.InverseTransformDirection(-rb.velocity);
        Vector3 veloEuler = veloLocal;

        veloEuler.x = veloLocal.z * 0.015f;
        veloEuler.z = veloLocal.x * 0.6f;
        veloEuler.y = veloLocal.x * 0.5f;

        veloEuler = Vector3.ClampMagnitude(veloEuler, 2);

        carBody.transform.eulerAngles += veloEuler * 0.925f;
        carBody.transform.localRotation = Quaternion.Lerp(carBody.transform.localRotation, Quaternion.Euler(targetEuler), 11 * Time.deltaTime);
        carBody.transform.localPosition += Vector3.up * (Mathf.Abs(veloEuler.x * 0.01f) + Mathf.Abs(veloEuler.z * 0.01f));
    }

    IEnumerator InitialAccelerationBounce()
    {
        float timer = 0;

        while (timer < 0.1f)
        {
            carBody.transform.localEulerAngles -= Vector3.right * (2 - timer * 2) * (0.5f * 0.45f);
            timer += Time.deltaTime;
            yield return new WaitForSeconds(0.01f);
        }
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
        if (!wheelIsGrounded[0] && !wheelIsGrounded[1] && rb.velocity.magnitude<2)
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

        if (col.contacts[0].normal.y>0.7f)
        {
            StartCoroutine("InitialAccelerationBounce");
        }
        else
        {
            //Debug.Log(rb.velocity);
        }
    }

    void RotateWheel(Transform wheel)
    {
        wheel.Rotate(Vector3.right * -currentWheelSpeed * 2);
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

        ret = Physics.SphereCast(wheel.transform.position, myInfo.wheelSize * 0.3f, -transform.up, out wheelRaycasts[index], myInfo.wheelSize, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);
        
        if(ret)
        {
            Debug.DrawLine(wheel.transform.position, wheelRaycasts[index].point, Color.red, 0.01f);
        }
        else
        {
            Debug.DrawLine(wheel.transform.position, wheel.transform.position -transform.up* myInfo.wheelSize, Color.blue, 0.02f);
        }

        return ret;
    }
}
