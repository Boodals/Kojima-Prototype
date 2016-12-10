﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TrapSpawner : MonoBehaviour {

    public bool spawnTraps;
    public float spawnInterval;
    public float currentTimer = 0.0f;    
    public GameObject spawnObject;
    public Text nextTrap;

    int randomArrayIndex;

    //Quaternion trapRotation;
    Object[] traps;

    // Use this for initialization
    void Start () {
        if (spawnTraps == true)
        {
            traps = Resources.LoadAll("Traps");
            randomArrayIndex = Random.Range(0, traps.Length);
            nextTrap.text = traps[randomArrayIndex].name;
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (spawnTraps == true)
        {
            currentTimer += Time.deltaTime;

            if (currentTimer >= spawnInterval)
            {
                currentTimer = 0.0f;
                //trapRotation = new Quaternion(Quaternion.identity.x, Quaternion.identity.y, gameObject.transform.rotation.z, gameObject.transform.rotation.w);
                Instantiate(traps[randomArrayIndex], spawnObject.transform.position, Quaternion.identity);
                randomArrayIndex = Random.Range(0, traps.Length);
                nextTrap.text = traps[randomArrayIndex].name;
            }
        }
	}
}
