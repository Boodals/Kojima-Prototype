using UnityEngine;
using System.Collections;

namespace Bam
{
    public class CarSoundScript : MonoBehaviour
    {
        Kojima.CarScript m_myCar;

        //Audio
        AudioSource m_engine, m_acceleration, m_otherSounds, m_skidSounds;
        public AudioClip m_smallImpact, m_mediumImpact;
        public AudioClip m_skidSnd;

        void Awake()
        {
            //Create audio sources
            m_engine = gameObject.AddComponent<AudioSource>();
            m_engine.spatialBlend = 0.0f;
            m_engine.loop = true;
            m_engine.volume = 0.0f;

            m_acceleration = gameObject.AddComponent<AudioSource>();
            m_acceleration.spatialBlend = 0.0f;
            m_acceleration.loop = true;

            m_otherSounds = gameObject.AddComponent<AudioSource>();
            m_otherSounds.spatialBlend = 0.0f;

            m_skidSounds = gameObject.AddComponent<AudioSource>();
            m_skidSounds.loop = true;
            m_skidSounds.clip = m_skidSnd;
            m_skidSounds.volume = 0.0f;
            m_skidSounds.Play();
        }

        // Use this for initialization
        void Start()
        {
            m_myCar = GetComponent<Kojima.CarScript>();
        }

        // Update is called once per frame
        void Update()
        {
            HandleSkidding();
        }

        void HandleSkidding()
        {
            float skidIntensity = m_myCar.GetSkidInfo();
            skidIntensity = Mathf.Clamp(skidIntensity, 0, 1.1f);

            m_skidSounds.volume = Mathf.Lerp(m_skidSounds.volume, skidIntensity * 0.5f, 5 * Time.deltaTime);
            //skidSounds.pitch = Mathf.Lerp(skidSounds.pitch, 0.8f + (skidIntensity * 0.3f), 0.1f * Time.deltaTime);

            if (m_myCar.InMidAir)
            {
                //skidSounds.volume = 0;
            }
        }

        void FixedUpdate()
        {

        }

        public void WheelHasLanded(float intensity = 1)
        {
            //otherSounds.PlayOneShot(smallImpact, 0.25f);
        }

        public void SetSounds(AudioClip _engine, AudioClip _acceleration)
        {
            m_engine.clip = _engine;
            m_engine.Play();

            m_acceleration.clip = _acceleration;
            m_acceleration.Stop();
        }
    }
}