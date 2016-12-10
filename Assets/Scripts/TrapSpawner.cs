using UnityEngine;
using System.Collections;

public class TrapSpawner : MonoBehaviour {

    public bool spawnTraps;
    public float spawnInterval;
    public float currentTimer = 0.0f;    
    public GameObject spawnObject;

    //Quaternion trapRotation;
    Object[] traps;

    // Use this for initialization
    void Start () {
        traps = Resources.LoadAll("Traps");
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
                Instantiate(traps[Random.Range(0, traps.Length)], spawnObject.transform.position, Quaternion.identity);
            }
        }
	}
}
