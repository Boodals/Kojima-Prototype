using UnityEngine;
using System.Collections;

public class CarSoundScript : MonoBehaviour
{
    CarScript myCar;

    //Audio
    AudioSource engine, acceleration, otherSounds, skidSounds;
    public AudioClip smallImpact, mediumImpact;
    public AudioClip skidSnd;

    void Awake()
    {
        //Create audio sources
        engine = gameObject.AddComponent<AudioSource>();
        engine.spatialBlend = 0;
        engine.loop = true;
        engine.volume = 0.05f;

        acceleration = gameObject.AddComponent<AudioSource>();
        acceleration.spatialBlend = 0;
        acceleration.loop = true;

        otherSounds = gameObject.AddComponent<AudioSource>();
        otherSounds.spatialBlend = 0;

        skidSounds = gameObject.AddComponent<AudioSource>();
        skidSounds.loop = true;
        skidSounds.clip = skidSnd;
        skidSounds.volume = 0;
        skidSounds.Play();
    }

    // Use this for initialization
    void Start()
    {
        myCar = GetComponent<CarScript>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleSkidding();
    }

    void HandleSkidding()
    {
        float skidIntensity = myCar.GetSkidInfo();
        skidIntensity = Mathf.Clamp(skidIntensity, 0, 1.1f);

        skidSounds.volume = Mathf.Lerp(skidSounds.volume, skidIntensity * 0.5f, 5 * Time.deltaTime);
        //skidSounds.pitch = Mathf.Lerp(skidSounds.pitch, 0.8f + (skidIntensity * 0.3f), 0.1f * Time.deltaTime);

        if (myCar.InMidAir)
        {
            //skidSounds.volume = 0;
        }
    }

    void FixedUpdate()
    {

    }

    public void WheelHasLanded(float intensity = 1)
    {
        otherSounds.PlayOneShot(smallImpact, 0.25f);
    }

    public void SetSounds(AudioClip _engine, AudioClip _acceleration)
    {
        engine.clip = _engine;
        engine.Play();

        acceleration.clip = _acceleration;
        acceleration.Stop();
    }
}
