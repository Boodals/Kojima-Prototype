//Author:       TMS
//Description:  Script that controls how a car behaves. 
//              Acts as a "Player Controller" for the car.
//Last edit:    TMS @ 16/01/2017

using UnityEngine;
using System.Collections;
using System;

namespace Kojima
{
    [RequireComponent(typeof(CarResetter))]
    public class CarScript : MonoBehaviour
    {
        Bam.CarSoundScript m_mySoundScript;

        [System.Serializable]
        public struct CarInfo_s
        {
            public enum driveMode_e { rearWheels, frontWheels, allWheels };
            public driveMode_e m_myDriveMode;

            public float m_fhealth, m_facceleration, m_fturnSpeed, m_fwheelSize;
            public AudioClip m_engineAudioClip, m_accelerationAudioClip;

            public bool m_airControl;

            public CarInfo_s(float _health, float _acceleration, float _turnSpeed, float _wheelSize, driveMode_e _driveMode, string engineSoundFilename = "Engine1", string accelerationSoundFileName = "Acceleration1")
            {
                m_myDriveMode = _driveMode;
                m_fhealth = _health;
                m_facceleration = _acceleration;
                m_fturnSpeed = _turnSpeed;
                m_fwheelSize = _wheelSize;

                m_engineAudioClip = Resources.Load<AudioClip>("Sounds/Engines/" + engineSoundFilename);
                //Debug.Log("Sounds/Engines/" + engineSoundFilename);

                m_accelerationAudioClip = Resources.Load<AudioClip>("Sounds/Acceleration/" + accelerationSoundFileName);
                //Debug.Log("Sounds/Acceleration/" + accelerationSoundFileName);

                m_airControl = true;
            }
        }

        public static bool s_playersCanMove = true;
        private bool m_canIMove = true;
        public bool CanMove
        {
            get { return m_canIMove; }
            set { m_canIMove = value; }
        }

        public int m_nplayerIndex = 1;

        [HideInInspector]
        CarInfo_s m_myInfo;

        public Transform m_carBody;

        [Tooltip("BL, BR, FL, FR")]
        public Transform[] m_wheels;
        public GameObject m_wheelLandParticlePrefab;
        ParticleSystem[] m_wheelLandParticles;
        public bool[] m_wheelIsGrounded;

        /// <summary>
        /// Returns true if all the wheels are grounded
        /// </summary>
        public bool AllWheelsGrounded
        {
            //@Assumes 4 wheels, can be improved if other vehicles are required.
            get { return (m_wheelIsGrounded[0] && m_wheelIsGrounded[1] && m_wheelIsGrounded[2] && m_wheelIsGrounded[3]); }
        }
        /// <summary>
        /// Returns true if all the wheels are NOT grounded
        /// </summary>
        public bool InMidAir
        {
            //@Assumes 4 wheels, can be improved if other vehicles are required.
            get { return (!m_wheelIsGrounded[0] && !m_wheelIsGrounded[1] && !m_wheelIsGrounded[2] && !m_wheelIsGrounded[3]); }
        }
        private bool m_inWater = false;
        public bool InWater
        {
            get { return m_inWater; }
        }

        RaycastHit[] m_wheelRaycasts;
        public float m_fwheelTorque;

        public float m_fcurrentWheelSpeed = 0;

        Vector3[] m_wheelLocalPositions;

        bool m_currentlySkidding = false;
        float m_fcurSkidIntensity = 0;

        TrailRenderer[] m_skidMarkTrails;
        public GameObject m_skidMarkPrefab;
        Vector3 m_skidDirection;

        Rigidbody m_rb;

        Vector3 m_bodyVelocity, m_bodyAngularVelocity;

        float m_fflipTimer = 0;

        //Don't worry about these
        [HideInInspector]
        public string m_strplayerInputTag;

        [SerializeField]
        bool m_drifting = false;
        Vector3 m_driftVelo;

        float m_currentSpeedMultiplier = 1;

        float m_ftargetAngularDrag = 5;
        float m_fcancelHoriForce = 20;

        public ParticleSystem m_skidSmoke;

        private CarResetter mRef_carResetter;
        private CapsuleCollider mRef_collider;
        public CapsuleCollider m_carCollider { get { return mRef_collider; } }

        private Bam.CarSoundScript m_soundScript;

        // Use this for initialization
        void Awake()
        {
            if(m_nplayerIndex==0)
            {
                m_nplayerIndex = GameController.s_ncurrentPlayers+1;
                Debug.LogWarning("Player index auto-corrected to " + m_nplayerIndex + " on " + gameObject.name + "!");
                //Debug.Break();

            }

            m_strplayerInputTag = "_P" + m_nplayerIndex;

            m_wheelLocalPositions = new Vector3[m_wheels.Length];
            m_wheelIsGrounded = new bool[m_wheels.Length];
            m_wheelRaycasts = new RaycastHit[m_wheels.Length];

            for (int i = 0; i < m_wheelLocalPositions.Length; i++)
            {
                m_wheelLocalPositions[i] = m_wheels[i].transform.localPosition;
            }

            m_rb = GetComponent<Rigidbody>();

            m_bodyVelocity = Vector3.zero;
            m_bodyAngularVelocity = Vector3.zero;

            GameController.s_ncurrentPlayers++;
            GameController.s_singleton.m_players[m_nplayerIndex - 1] = this;

            //Cache a reference to the CarRestter script
            mRef_carResetter = GetComponent<CarResetter>();
            mRef_collider = GetComponent<CapsuleCollider>();

            //Cache a reference to the CarSound script
            m_soundScript = GetComponent<Bam.CarSoundScript>();


            //Create wheel land effects
            m_wheelLandParticles = new ParticleSystem[m_wheels.Length];
            for (int i = 0; i < m_wheels.Length; i++)
            {
                m_wheelLandParticles[i] = Instantiate<GameObject>(m_wheelLandParticlePrefab).GetComponent<ParticleSystem>();
                m_wheelLandParticles[i].transform.SetParent(m_wheels[i]);
                m_wheelLandParticles[i].transform.localPosition = Vector3.zero;
                m_wheelLandParticles[i].transform.localEulerAngles = new Vector3(-90, 0, 0);
                m_wheelLandParticles[i].transform.localScale = Vector3.one;
            }

            m_mySoundScript = GetComponent<Bam.CarSoundScript>();
        }

        void Start()
        {
            ApplyCarInfo(new CarInfo_s(100, 22, 12, 0.35f, CarInfo_s.driveMode_e.allWheels, "Engine1"));
            CreateSkidMarkTrails();

            m_skidSmoke.Stop();
        }

        void CreateSkidMarkTrails()
        {
            m_skidMarkTrails = new TrailRenderer[m_wheels.Length];

            for (int i = 0; i < m_wheels.Length; i++)
            {
                m_skidMarkTrails[i] = GameObject.Instantiate<GameObject>(m_skidMarkPrefab).GetComponent<TrailRenderer>();
                m_skidMarkTrails[i].enabled = false;
                m_skidMarkTrails[i].transform.SetParent(m_wheels[i], true);
                m_skidMarkTrails[i].transform.localPosition = m_wheels[i].transform.position - (Vector3.up * m_myInfo.m_fwheelSize);
                m_skidMarkTrails[i].enabled = true;
            }
        }

        void ManageSkidMarkTrails()
        {
            if (m_drifting)
            {
                for (int i = 0; i < m_skidMarkTrails.Length; i++)
                {
                    m_skidMarkTrails[i].transform.position = m_wheels[i].transform.position - (Vector3.up * m_myInfo.m_fwheelSize);
                }
            }
            else
            {
                DisconnectSkidMarkTrails();
            }
        }

        void DisconnectSkidMarkTrails()
        {
            for (int i = 0; i < m_skidMarkTrails.Length; i++)
            {
                if (m_skidMarkTrails[i])
                {
                    m_skidMarkTrails[i].transform.SetParent(null);
                    Destroy(m_skidMarkTrails[i], 30);
                }
            }

            m_skidMarkTrails = new TrailRenderer[0];
        }

        void OnDestroy()
        {
            GameController.s_ncurrentPlayers--;
        }

        public void ApplyCarInfo(CarInfo_s newInfo)
        {
            m_myInfo = newInfo;
            m_soundScript.SetSounds(m_myInfo.m_engineAudioClip, m_myInfo.m_accelerationAudioClip);
        }

        public Vector3 GetVelocity()
        {
            return m_rb.velocity;
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
                m_inWater = true;

                transform.position = mRef_carResetter.GetLastSafePosition();
                //mRef_CarResetter.ResetRecord();
                mRef_carResetter.ForceRecord();
                m_rb.velocity = Vector3.zero;
                m_rb.angularVelocity = Vector3.zero;
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
                m_inWater = false;
            }
        }
        // Update is called once per frame
        void FixedUpdate()
        {
            Movement();

            if (InMidAir)
            {
                m_fcurSkidIntensity = 0;

                if (m_skidSmoke.isPlaying)
                {
                    m_skidSmoke.Stop();
                }

                if (m_myInfo.m_airControl && Mathf.Abs(m_rb.velocity.y) > 1)
                {
                    AirControl();
                }
            }

            //Manage drag
            m_rb.drag = 0.15f;

            for(int i=0; i<m_wheels.Length; i++)
            {
                if(m_wheelIsGrounded[i])
                {
                    m_rb.drag += 0.25f;
                }
            }
        }

        public float GetSkidInfo()
        {
            return m_fcurSkidIntensity;
        }

        void AirControl()
        {
            Vector3 controlVelocity = new Vector3(-Input.GetAxisRaw("Vertical" + m_strplayerInputTag), Input.GetAxisRaw("Horizontal" + m_strplayerInputTag), 0);
            controlVelocity = transform.TransformDirection(controlVelocity);
            m_rb.AddTorque(controlVelocity * 5, ForceMode.Acceleration);
        }

        void Movement()
        {
            float currentVelocity = m_rb.velocity.magnitude;
            m_fcurrentWheelSpeed = currentVelocity * Vector3.Dot(m_rb.velocity.normalized, -transform.forward);
            ManageSkidMarkTrails();
            m_rb.angularDrag = 0.5f;

            if (s_playersCanMove && m_canIMove)
            {
                //Drifting
                m_drifting = Input.GetKey("joystick " + m_nplayerIndex + " button 1") && m_wheelIsGrounded[0] && m_wheelIsGrounded[1];

                if (Input.GetKey("joystick " + m_nplayerIndex + " button 1"))
                {
                    m_skidDirection = transform.forward;
                }

                if (m_drifting)
                {
                    m_fcancelHoriForce = 0;
                }
                else
                {
                    m_fcancelHoriForce = Mathf.Lerp(m_fcancelHoriForce, 1, 0.5f * Time.deltaTime);
                }

                //Give each wheel a chance to push the car if grounded
                for (int i = 0; i < m_wheels.Length; i++)
                {
                    m_wheelIsGrounded[i] = isWheelGrounded(m_wheels[i], i);

                    bool thisWheelCanDrive = false;

                    switch (m_myInfo.m_myDriveMode)
                    {
                        case CarInfo_s.driveMode_e.allWheels:
                            thisWheelCanDrive = true;
                            break;
                        case CarInfo_s.driveMode_e.rearWheels:
                            if (i <= 1)
                            {
                                thisWheelCanDrive = true;
                            }
                            break;
                        case CarInfo_s.driveMode_e.frontWheels:
                            if (i > 1)
                            {
                                thisWheelCanDrive = true;
                            }
                            break;
                    }

                    if (!m_wheelIsGrounded[i])
                    {
                        thisWheelCanDrive = false;
                        m_drifting = false;
                    }

                    if (m_wheelIsGrounded[i])
                    {
                        PreventSkidding();

                        //Adds some angular drag for each grounded wheel
                        if ((m_drifting && i > 1) || !m_drifting)
                        {
                            m_rb.angularDrag += m_ftargetAngularDrag / 4;
                        }
                    }

                    float forwardsMultiplier = Input.GetAxisRaw("Acceleration" + m_strplayerInputTag) + (-Input.GetAxisRaw("Brake" + m_strplayerInputTag));

                    if (thisWheelCanDrive)
                    {
                        if (forwardsMultiplier != 0)
                        {
                            Vector3 wheelForward = transform.forward;

                            Vector3 direction = Vector3.Cross(m_wheelRaycasts[i].normal, wheelForward);
                            direction = Vector3.Cross(direction, m_wheelRaycasts[i].normal);

                            Debug.DrawLine(m_wheels[i].transform.position, m_wheels[i].transform.position + direction * 5, Color.green);

                            Vector3 accelerationForce = direction * m_myInfo.m_facceleration * Input.GetAxisRaw("Acceleration" + m_strplayerInputTag);
                            Vector3 brakeForce = -direction * m_myInfo.m_facceleration * Input.GetAxisRaw("Brake" + m_strplayerInputTag) * 0.55f;

                            if (m_myInfo.m_myDriveMode == CarInfo_s.driveMode_e.allWheels)
                            {
                                accelerationForce *= 0.5f;
                                brakeForce *= 0.5f;
                            }

                            m_rb.AddForce(accelerationForce, ForceMode.Acceleration);
                            m_rb.AddForce(brakeForce, ForceMode.Acceleration);

                            m_rb.rotation = Quaternion.RotateTowards(m_rb.rotation, Quaternion.LookRotation(direction, m_wheelRaycasts[i].normal), 35 * Time.deltaTime);
                        }

                    }

                    //Stabiliser forces
                    m_rb.AddForce(-m_wheelRaycasts[i].normal * m_fcurrentWheelSpeed * 15);
                    m_rb.AddForce(-Vector3.up * 10);

                    if (i > 1)
                    {
                        float curTorqueSpeed = m_myInfo.m_fturnSpeed * m_fcurrentWheelSpeed;

                        curTorqueSpeed = Mathf.Clamp(curTorqueSpeed, -m_myInfo.m_fturnSpeed, m_myInfo.m_fturnSpeed);

                        if (m_wheelIsGrounded[i])
                        {
                            m_rb.AddTorque(transform.up * Input.GetAxisRaw("Horizontal" + m_strplayerInputTag) * (curTorqueSpeed), ForceMode.Acceleration);
                        }
                    }
                }
            }
        }

        void LateUpdate()
        {
            for (int i = 0; i < m_wheels.Length; i++)
            {
                RotateWheel(m_wheels[i]);

                if (i > 1)
                {
                    float lerpValue = 0.5f + Input.GetAxis("Horizontal" + m_strplayerInputTag) * 0.5f;
                    float newY = Mathf.Lerp(-35, 35, lerpValue);

                    m_wheels[i].transform.localRotation = Quaternion.Lerp(m_wheels[i].transform.localRotation, Quaternion.Euler(new Vector3(m_wheels[i].localEulerAngles.x, newY, 0)), 8 * Time.deltaTime);
                }
            }

            SuspensionEffects();
        }

        protected virtual void SuspensionEffects()
        {
            //Suspension
            m_carBody.transform.position += Vector3.up * -m_rb.velocity.y * 0.1f;

            m_carBody.transform.localPosition = Vector3.ClampMagnitude(m_carBody.transform.localPosition * 0.01f, 0.15f);
            m_carBody.transform.localPosition = Vector3.Lerp(m_carBody.transform.localPosition, Vector3.zero, 1 * Time.deltaTime);

            Vector3 targetEuler = Vector3.zero;
            Vector3 veloLocal = transform.InverseTransformDirection(-m_rb.velocity);
            Vector3 veloEuler = veloLocal;

            veloEuler.x = veloLocal.z * 0.015f;
            veloEuler.z = veloLocal.x * 0.4f;
            veloEuler.y = veloLocal.x * 0.5f;

            veloEuler = Vector3.ClampMagnitude(veloEuler, 2);

            m_carBody.transform.eulerAngles += veloEuler * 0.925f;
            m_carBody.transform.localRotation = Quaternion.Lerp(m_carBody.transform.localRotation, Quaternion.Euler(targetEuler), 11 * Time.deltaTime);
            m_carBody.transform.localPosition += Vector3.up * (Mathf.Abs(veloEuler.x * 0.01f) + Mathf.Abs(veloEuler.z * 0.01f));
        }

        IEnumerator InitialAccelerationBounce()
        {
            float timer = 0;

            while (timer < 0.1f)
            {
                m_carBody.transform.localEulerAngles -= Vector3.right * (2 - timer * 2) * (0.5f * 0.45f);
                timer += Time.deltaTime;
                yield return new WaitForSeconds(0.01f);
            }
        }

        IEnumerator RollBackOver()
        {
            //This timer is here just in case this loop never ends naturally
            float timer = 5;

            while (transform.up.y < 0.995f && timer > 0)
            {
                timer -= Time.deltaTime;
                m_rb.rotation = Quaternion.Lerp(m_rb.rotation, Quaternion.LookRotation(transform.forward, Vector3.up), 5 * Time.deltaTime);
                yield return new WaitForSeconds(0.01f);
            }

            m_rb.useGravity = true;
        }

        void OnCollisionStay()
        {
            if (!m_wheelIsGrounded[0] && !m_wheelIsGrounded[1] && m_rb.velocity.magnitude < 2)
            {
                m_fflipTimer += Time.deltaTime;

                if (m_fflipTimer > 3)
                {
                    StopCoroutine("RollBackOver");
                    StartCoroutine("RollBackOver");
                }
            }
            else
            {
                m_fflipTimer = 0;
            }
        }


        void OnCollisionEnter(Collision col)
        {
            m_carBody.transform.position += m_rb.velocity * 0.01f;
            float intensity = col.relativeVelocity.magnitude;

            //Debug.Log(intensity);

            if (Vector3.Dot(transform.forward, col.contacts[0].normal) < -0.2f)
            {
                //StartCoroutine("InitialAccelerationBounce");
            }
            else
            {
                //Debug.Log(rb.velocity);
            }
        }

        void RotateWheel(Transform wheel)
        {
            wheel.Rotate(Vector3.right * -m_fcurrentWheelSpeed * 2);
        }

        void PreventSkidding()
        {
            Vector3 velo = m_rb.velocity;
            Vector3 localVelo = transform.InverseTransformDirection(velo);

            float speedSimilarity = Mathf.Abs(Vector3.Dot(transform.forward, m_rb.velocity));

            if (Mathf.Abs(localVelo.x) > 4f)
            {
                m_currentlySkidding = true;
                m_fcurSkidIntensity = Mathf.Abs(localVelo.x);

                if (!m_skidSmoke.isPlaying)
                    m_skidSmoke.Play();
            }
            else
            {
                m_currentlySkidding = false;
                m_fcurSkidIntensity = 0;

                if (m_skidSmoke.isPlaying)
                    m_skidSmoke.Stop();
            }

            m_fwheelTorque = localVelo.z;
            localVelo.x = Mathf.Lerp(localVelo.x, 0, m_fcancelHoriForce * (1) * Time.deltaTime);

            m_rb.velocity = transform.TransformDirection(localVelo);
        }

        bool isWheelGrounded(Transform wheel, int index)
        {
            bool ret;
            bool previouslyGrounded = m_wheelIsGrounded[index];

            ret = Physics.SphereCast(wheel.transform.position, m_myInfo.m_fwheelSize * 0.15f, -transform.up, out m_wheelRaycasts[index], m_myInfo.m_fwheelSize, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);

            if (ret)
            {
                Debug.DrawLine(wheel.transform.position, m_wheelRaycasts[index].point, Color.red, 0.01f);
            }
            else
            {
                Debug.DrawLine(wheel.transform.position, wheel.transform.position - transform.up * m_myInfo.m_fwheelSize, Color.blue, 0.02f);
            }

            if (ret && !previouslyGrounded)
            {
                WheelHasLanded(index);
            }

            return ret;
        }

        void WheelHasLanded(int index)
        {
            m_mySoundScript.WheelHasLanded();

            if (m_rb.velocity.y < -1)
                m_wheelLandParticles[index].Play();
        }
    }
}