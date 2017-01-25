using UnityEngine;
using System.Collections;

public class CarSoundScript : MonoBehaviour
{

    //Audio
    AudioSource engine, acceleration, otherSounds;
    public AudioClip smallImpact, mediumImpact;

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
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {

    }

    public void SetSounds(AudioClip _engine, AudioClip _acceleration)
    {
        engine.clip = _engine;
        engine.Play();

        acceleration.clip = _acceleration;
        acceleration.Stop();
    }
}
