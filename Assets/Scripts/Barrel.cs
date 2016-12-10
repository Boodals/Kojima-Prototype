using UnityEngine;
using System.Collections;

public class Barrel : MonoBehaviour {

	// Use this for initialization
	void Start () {
        gameObject.transform.rotation = new Quaternion(-90, gameObject.transform.rotation.y, gameObject.transform.rotation.z, gameObject.transform.rotation.w);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
