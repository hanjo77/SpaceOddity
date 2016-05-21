using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MarsBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start () {
		// Debug.Log ("wake up!");
	}

	void OnTriggerEnter(Collider other) {
		MeshExploder meshExploder = other.transform.GetComponent<MeshExploder> ();
		if (other.name != "laser") {
			ArwingController arwing = other.GetComponentInParent<ArwingController> ();
			if (meshExploder != null) {
				meshExploder.Explode ();
				if (arwing) {
					if (arwing.isLocalPlayer) {
						Transform camera = GameObject.Find("camera").transform;
						camera.position = arwing.transform.position;
						camera.rotation = arwing.transform.rotation;
					}
					// GameObject.Destroy(arwing.transform.gameObject);
				}
			}
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
