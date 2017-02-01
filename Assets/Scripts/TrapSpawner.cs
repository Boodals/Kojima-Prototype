using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Bam
{
    public class TrapSpawner : MonoBehaviour
    {

        public bool spawnTraps;
        public float spawnInterval;
        public float currentTimer = 0.0f;
        public Vector3 spawnPosition;
        public int playerID;
        //public Text nextTrap;

        int randomArrayIndex;

        //Quaternion trapRotation;
        Object[] traps;

        // Use this for initialization
        void Start()
        {
            playerID = gameObject.GetComponent<Kojima.CarScript>().m_nplayerIndex;

            if (spawnTraps == true)
            {
                traps = Resources.LoadAll("Traps");
                randomArrayIndex = Random.Range(0, traps.Length);
                MainHUDScript.singleton.ShowNextItem(playerID, traps[randomArrayIndex].name);
                //nextTrap.text = traps[randomArrayIndex].name;
            }
        }



        // Update is called once per frame
        void Update()
        {
            if (spawnTraps == true)
            {
                currentTimer += Time.deltaTime;

                if (currentTimer >= spawnInterval)
                {
                    currentTimer = 0.0f;
                    //trapRotation = new Quaternion(Quaternion.identity.x, Quaternion.identity.y, gameObject.transform.rotation.z, gameObject.transform.rotation.w);
                    Instantiate(traps[randomArrayIndex], transform.position - -transform.forward * 6 + transform.up, Quaternion.LookRotation(transform.up, transform.forward));
                    randomArrayIndex = Random.Range(0, traps.Length);
                    MainHUDScript.singleton.ShowNextItem(playerID, traps[randomArrayIndex].name);
                }
            }
        }
    }
}