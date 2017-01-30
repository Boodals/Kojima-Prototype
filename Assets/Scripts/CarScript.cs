//Author:       TMS
//Description:  Script that controls how a car behaves. 
//              Acts as a "Player Controller" for the car.
//Last edit:    TMS @ 16/01/2017

using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(CarResetter))]
public class CarScript : MonoBehaviour
{
    CarSoundScript mySoundScript;

    [System.Serializable]
    public struct CarInfo
    {
        public enum DriveMode { RearWheels, FrontWheels, AllWheels};
        public DriveMode myDriveMode;

        public float health, acceleration, turnSpeed, wheelSize;
        public AudioClip engineAudioClip, accelerationAudioClip;

        public bool airControl;

        public CarInfo(float _health, float _acceleration, float _turnSpeed, float _wheelSize, DriveMode _driveMode, string engineSoundFilename = "Engine1", string accelerationSoundFileName = "Acceleration1")
        {
            myDriveMode = _driveMode;
            health = _health;
            acceleration = _acceleration;
            turnSpeed = _turnSpeed;
            wheelSize = _wheelSize;

            engineAudioClip = Resources.Load<AudioClip>("Sounds/Engines/" + engineSoundFilename);
            Debug.Log("Sounds/Engines/" + engineSoundFilename);

            accelerationAudioClip = Resources.Load<AudioClip>("Sounds/Acceleration/" + accelerationSoundFileName);
            Debug.Log("Sounds/Acceleration/" + accelerationSoundFileName);

            airControl = true;
        }
    }

    public static bool playersCanMove = true;
    private bool canIMove = true;
    public bool CanMove
    {
        get { return canIMove; }
        set { canIMove = value; }
    }

    public int playerIndex = 1;

    public CarInfo myInfo;

    public Transform carBody;

    [Tooltip("BL, BR, FL, FR")]
    public Transform[] wheels;
    public GameObject wheelLandParticlePrefab;
    ParticleSystem[] wheelLandParticles;
    public bool[] wheelIsGrounded;

    /// <summary>
    /// Returns true if all the wheels are grounded
    /// </summary>
    public bool AllWheelsGrounded
    {
        //@Assumes 4 wheels, can be improved if other vehicles are required.
        get { return (wheelIsGrounded[0] && wheelIsGrounded[1] && wheelIsGrounded[2] && wheelIsGrounded[3]); }
    }
    /// <summary>
    /// Returns true if all the wheels are NOT grounded
    /// </summary>
    public bool InMidAir
    {
        //@Assumes 4 wheels, can be improved if other vehicles are required.
        get { return (!wheelIsGrounded[0] && !wheelIsGrounded[1] && !wheelIsGrounded[2] && !wheelIsGrounded[3]); }
    }
    private bool inWater = false;
    public bool InWater
    {
        get { return inWater; }
    }

    RaycastHit[] wheelRaycasts;
    public float wheelTorque;

    public float currentWheelSpeed = 0;

    Vector3[] wheelLocalPositions;

    bool currentlySkidding = false;
    float curSkidIntensity = 0;

    TrailRenderer[] skidMarkTrails;
    public GameObject skidMarkPrefab;
    Vector3 skidDirection;

    Rigidbody rb;

    Vector3 bodyVelocity, bodyAngularVelocity;

    float flipTimer = 0;

    //Don't worry about these
    [HideInInspector]
    public string playerInputTag;

    [SerializeField]
    bool drifting = false;
    Vector3 driftVelo;

    float currentSpeedMultiplier = 1;

    float targetAngularDrag = 5;

    public float cancelHoriForce = 20;

    public ParticleSystem skidSmoke;

    private CarResetter mRef_carResetter;
    private CapsuleCollider mRef_collider;
    public CapsuleCollider CarCollider { get { return mRef_collider;  } }

    private CarSoundScript soundScript;

    // Use this for initialization
    void Awake()
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

        GameController.currentPlayers++;
        GameController.singleton.players[playerIndex-1] = this;
        
        //Cache a reference to the CarRestter script
        mRef_carResetter = GetComponent<CarResetter>();
        mRef_collider = GetComponent<CapsuleCollider>();

        //Cache a reference to the CarSound script
        soundScript = GetComponent<CarSoundScript>();


        //Create wheel land effects
        wheelLandParticles = new ParticleSystem[wheels.Length];
        for(int i=0; i<wheels.Length; i++)
        {
            wheelLandParticles[i] = Instantiate<GameObject>(wheelLandParticlePrefab).GetComponent<ParticleSystem>();
            wheelLandParticles[i].transform.SetParent(wheels[i]);
            wheelLandParticles[i].transform.localPosition = Vector3.zero;
            wheelLandParticles[i].transform.localEulerAngles = new Vector3(-90, 0, 0);
            wheelLandParticles[i].transform.localScale = Vector3.one;
        }

        mySoundScript = GetComponent<CarSoundScript>();
    }

    void Start()
    {
        ApplyCarInfo(new CarInfo(100, 15, 12, 0.35f, CarInfo.DriveMode.AllWheels, "Engine1"));
        CreateSkidMarkTrails();

        skidSmoke.Stop();
    }

    void CreateSkidMarkTrails()
    {
        skidMarkTrails = new TrailRenderer[wheels.Length];

        for(int i=0; i<wheels.Length; i++)
        {
            skidMarkTrails[i] = GameObject.Instantiate<GameObject>(skidMarkPrefab).GetComponent<TrailRenderer>();
            skidMarkTrails[i].enabled = false;
            skidMarkTrails[i].transform.SetParent(wheels[i], true);
            skidMarkTrails[i].transform.localPosition = wheels[i].transform.position - (Vector3.up * myInfo.wheelSize);
            skidMarkTrails[i].enabled = true;
        }
    }

    void ManageSkidMarkTrails()
    {
        if (drifting)
        {
            for (int i = 0; i < skidMarkTrails.Length; i++)
            {
                skidMarkTrails[i].transform.position = wheels[i].transform.position - (Vector3.up * myInfo.wheelSize);
            }
        }
        else
        {
            DisconnectSkidMarkTrails();
        }
    }

    void DisconnectSkidMarkTrails()
    {
        for (int i = 0; i < skidMarkTrails.Length; i++)
        {
            if (skidMarkTrails[i])
            {
                skidMarkTrails[i].transform.SetParent(null);
                Destroy(skidMarkTrails[i], 30);
            }
        }

        skidMarkTrails = new TrailRenderer[0];
    }

    void OnDestroy()
    {
        GameController.currentPlayers--;
    }

    public void ApplyCarInfo(CarInfo newInfo)
    {
        myInfo = newInfo;
        soundScript.SetSounds(myInfo.engineAudioClip, myInfo.accelerationAudioClip);
    }

    public Vector3 GetVelocity()
    {
        return rb.velocity;
    }


    ///// <summary>
    ///// Call to check whether the car is in water (expensive, so cache the result)
    ///// @Could be improved as right now it only checks below wheels (always DOWN)
    ///// and above the car on global axis (UP). Using a trigger collider to check this
    ///// would be easier!
    ///// </summary>
    ///// <returns></returns>
    //private bool CheckInWater()
    //{
    //    //Check below wheels
    //    RaycastHit hit;
    //    Ray ray = new Ray();
    //    foreach (Transform wheel in wheels)
    //    {
    //        ray.origin = wheel.position;
    //        ray.direction = Vector3.down;
    //        if (Physics.Raycast(ray, out hit, myInfo.wheelSize))
    //        {
    //            if (hit.collider.gameObject.tag == "Water")
    //            {
    //                return true;
    //            }
    //        }
    //    }

    //    //Check above car - probably won't ever happen...
    //    ray.origin = transform.position;
    //    ray.direction = Vector3.up;
    //    if (Physics.Raycast(ray, out hit, mRef_collider.radius, LayerMask.NameToLayer("Player")))
    //    {
    //        if (hit.collider.gameObject.tag == "Water")
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Water")
        {
            inWater = true;

            transform.position = mRef_carResetter.GetLastSafePosition();
            //mRef_CarResetter.ResetRecord();
            mRef_carResetter.ForceRecord();
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Vector3 euler = transform.eulerAngles;
            euler.x = 0;
            euler.z = 0;
            transform.rotation = Quaternion.Euler(euler);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Water")
        {
            inWater = false;
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        Movement();

        if (InMidAir)
        {
            curSkidIntensity = 0;

            if (skidSmoke.isPlaying)
            {
                skidSmoke.Stop();
            }

            if(myInfo.airControl && Mathf.Abs(rb.velocity.y)>1)
            {
                AirControl();
            }
        }
    }

    public float GetSkidInfo()
    {
        return curSkidIntensity;
    }

    void AirControl()
    {
        Vector3 controlVelocity = new Vector3(-Input.GetAxisRaw("Vertical" + playerInputTag), Input.GetAxisRaw("Horizontal" + playerInputTag), 0);
        controlVelocity = transform.TransformDirection(controlVelocity);
        rb.AddTorque(controlVelocity * 20, ForceMode.Acceleration);
    }

    void Movement()
    {
        float currentVelocity = rb.velocity.magnitude;
        currentWheelSpeed = currentVelocity * Vector3.Dot(rb.velocity.normalized, -transform.forward);
        ManageSkidMarkTrails();
        rb.angularDrag = 0;

        if (playersCanMove && canIMove)
        {
            //Drifting
            drifting = Input.GetKey("joystick " + playerIndex + " button 1") && wheelIsGrounded[0] && wheelIsGrounded[1];

            if (Input.GetKey("joystick " + playerIndex + " button 1"))
            {
                skidDirection = transform.forward;
            }

            if (drifting)
            {
                cancelHoriForce = 0;
            }
            else
            {
                cancelHoriForce = Mathf.Lerp(cancelHoriForce, 1, 0.5f * Time.deltaTime);
            }

            //Give each wheel a chance to push the car if grounded
            for (int i = 0; i < wheels.Length; i++)
            {
                wheelIsGrounded[i] = isWheelGrounded(wheels[i], i);

                bool thisWheelCanDrive = false;

                switch (myInfo.myDriveMode)
                {
                    case CarInfo.DriveMode.AllWheels:
                        thisWheelCanDrive = true;
                        break;
                    case CarInfo.DriveMode.RearWheels:
                        if (i <= 1)
                        {
                            thisWheelCanDrive = true;
                        }
                        break;
                    case CarInfo.DriveMode.FrontWheels:
                        if (i > 1)
                        {
                            thisWheelCanDrive = true;
                        }
                        break;
                }

                if (!wheelIsGrounded[i])
                {
                    thisWheelCanDrive = false;
                    drifting = false;
                }

                if (wheelIsGrounded[i])
                {
                    PreventSkidding();
                }

                float forwardsMultiplier = Input.GetAxisRaw("Acceleration" + playerInputTag) + (-Input.GetAxisRaw("Brake" + playerInputTag));

                //Adds some angular drag for each grounded wheel
                if ((drifting && i > 1) || !drifting)
                    rb.angularDrag += targetAngularDrag / 4;

                if (thisWheelCanDrive)
                {
                    if (forwardsMultiplier != 0)
                    {
                        Vector3 wheelForward = transform.forward;

                        Vector3 direction = Vector3.Cross(wheelRaycasts[i].normal, wheelForward);
                        direction = Vector3.Cross(direction, wheelRaycasts[i].normal);

                        Debug.DrawLine(wheels[i].transform.position, wheels[i].transform.position + direction * 5, Color.green);

                        Vector3 accelerationForce = direction * myInfo.acceleration * Input.GetAxisRaw("Acceleration" + playerInputTag);
                        Vector3 brakeForce = -direction * myInfo.acceleration * Input.GetAxisRaw("Brake" + playerInputTag) * 0.55f;

                        if (myInfo.myDriveMode == CarInfo.DriveMode.AllWheels)
                        {
                            accelerationForce *= 0.5f;
                            brakeForce *= 0.5f;
                        }

                        rb.AddForce(accelerationForce, ForceMode.Acceleration);
                        rb.AddForce(brakeForce, ForceMode.Acceleration);

                        rb.rotation = Quaternion.RotateTowards(rb.rotation, Quaternion.LookRotation(direction, wheelRaycasts[i].normal), 35 * Time.deltaTime);
                    }

                }

                //Stabiliser forces
                rb.AddForce(-wheelRaycasts[i].normal * currentWheelSpeed * 15);
                rb.AddForce(-Vector3.up * 10);

                if (i > 1)
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

        SuspensionEffects();
    }

    protected virtual void SuspensionEffects()
    {
        //Suspension
        carBody.transform.position += Vector3.up * -rb.velocity.y * 0.1f;

        carBody.transform.localPosition = Vector3.ClampMagnitude(carBody.transform.localPosition * 0.01f, 0.15f);
        carBody.transform.localPosition = Vector3.Lerp(carBody.transform.localPosition, Vector3.zero, 1 * Time.deltaTime);

        Vector3 targetEuler = Vector3.zero;
        Vector3 veloLocal = transform.InverseTransformDirection(-rb.velocity);
        Vector3 veloEuler = veloLocal;

        veloEuler.x = veloLocal.z * 0.015f;
        veloEuler.z = veloLocal.x * 0.4f;
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
        //This timer is here just in case this loop never ends naturally
        float timer = 5;

        while(transform.up.y<0.995f && timer>0)
        {
            timer -= Time.deltaTime;
            rb.rotation = Quaternion.Lerp(rb.rotation, Quaternion.LookRotation(transform.forward, Vector3.up), 5 * Time.deltaTime);
            yield return new WaitForSeconds(0.01f);
        }

        rb.useGravity = true;
    }

    void OnCollisionStay()
    {
        if (!wheelIsGrounded[0] && !wheelIsGrounded[1] && rb.velocity.magnitude<2)
        {
            flipTimer += Time.deltaTime;

            if (flipTimer > 3)
            {
                StopCoroutine("RollBackOver");
                StartCoroutine("RollBackOver");
            }
        }
        else
        {
            flipTimer = 0;
        }
    }


    void OnCollisionEnter(Collision col)
    {
        carBody.transform.position += rb.velocity * 0.01f;
        float intensity = col.relativeVelocity.magnitude;

        //Debug.Log(intensity);

        if (Vector3.Dot(transform.forward, col.contacts[0].normal)<-0.2f)
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

        if (Mathf.Abs(localVelo.x) > 4f)
        {
            currentlySkidding = true;
            curSkidIntensity = Mathf.Abs(localVelo.x);

            if (!skidSmoke.isPlaying)
                skidSmoke.Play();
        }
        else
        {
            currentlySkidding = false;
            curSkidIntensity = 0;

            if (skidSmoke.isPlaying)
                skidSmoke.Stop();
        }

        wheelTorque = localVelo.z;
        localVelo.x = Mathf.Lerp(localVelo.x, 0, cancelHoriForce * (1) * Time.deltaTime);

        rb.velocity = transform.TransformDirection(localVelo);
    }

    bool isWheelGrounded(Transform wheel, int index)
    {
        bool ret;
        bool previouslyGrounded = wheelIsGrounded[index];

        ret = Physics.SphereCast(wheel.transform.position, myInfo.wheelSize * 0.15f, -transform.up, out wheelRaycasts[index], myInfo.wheelSize, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);
        
        if(ret)
        {
            Debug.DrawLine(wheel.transform.position, wheelRaycasts[index].point, Color.red, 0.01f);
        }
        else
        {
            Debug.DrawLine(wheel.transform.position, wheel.transform.position -transform.up* myInfo.wheelSize, Color.blue, 0.02f);
        }

        if(ret && !previouslyGrounded)
        {
            WheelHasLanded(index);
        }

        return ret;
    }

    void WheelHasLanded(int index)
    {
        mySoundScript.WheelHasLanded();

        if (rb.velocity.y < -1)
            wheelLandParticles[index].Play();
    }
}
