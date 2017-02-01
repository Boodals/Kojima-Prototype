//Author:       Yams
//Description:  Script that is used to reset the car.
//Last edit:    Yams @ 14/01/2017 : removed unecessary complexity

using UnityEngine;
using System.Collections;

namespace Kojima
{
    [RequireComponent(typeof(CarScript))]
    public class CarResetter : MonoBehaviour
    {
        #region Public Members

        public float m_posRecordInterval;
        public bool m_debugLog;

        #endregion
        #region Private Members

        [SerializeField]
        private int MAX_POS_RECORD;
        //private CircularStack<Vector3>  m_positions;
        private Vector3 m_safePos;
        private Timer m_recordTimer;
        private bool m_shouldRecord;
        //Gotta love that tight coupling :S
        private CarScript mRef_carController;
        #endregion
        #region Public Methods

        public Vector3 GetLastSafePosition()
        {
            Debug.Log("Getting last safe car position.");
            return m_safePos;//m_positions.peekAtDepth(1);
        }

        //public void  ResetRecord()
        //{
        //    //m_positions.reset();
        //}

        public void ForceRecord()
        {
            // m_positions.push(transform.position);
            m_safePos = transform.position;
        }

        #endregion
        #region Private Methods

        private void Awake()
        {
            //m_positions = new CircularStack<Vector3>(MAX_POS_RECORD);
            // m_positions.push(transform.position);
            m_safePos = transform.position;
            m_recordTimer = new Timer();
            m_shouldRecord = false;
            mRef_carController = GetComponent<CarScript>();
        }

        private void Start()
        {

        }

        private void Update()
        {
            if (!m_shouldRecord && m_recordTimer.Elapsed() >= m_posRecordInterval)
            {
                m_shouldRecord = true;
            }
            else if (m_shouldRecord && mRef_carController.AllWheelsGrounded && !mRef_carController.InWater)
            {
                //m_positions.push(transform.position);
                m_safePos = transform.position;
                m_shouldRecord = false;
                m_recordTimer.Restart();

                if (m_debugLog) Debug.Log("Recording car position.");
            }
        }



        #endregion
    }
}