using UnityEngine;
using System.Collections;

namespace Bam
{
    [RequireComponent(typeof(Kojima.CarScript))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class KillIfOffscreen : MonoBehaviour
    {
        CapsuleCollider m_collider;
        void Awake()
        {
            m_collider = GetComponent<CapsuleCollider>();
        }


        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Kojima.CameraManagerScript.singleton.playerCameras[0].Cam);
            if (!GeometryUtility.TestPlanesAABB(planes, m_collider.bounds))
            {

            }
        }
    }
}