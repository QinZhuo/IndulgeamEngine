using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IndulgeamEngine.Network.ForUnity;
public class GameTest : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
        ScenceClient.Instantiate(0, new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f)), Quaternion.Euler(new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360))));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
