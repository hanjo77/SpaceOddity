using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class MarsBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start () {
		// Debug.Log ("wake up!");
	}

	void OnTriggerEnter(Collider other) {
		MeshExploder meshExploder = other.transform.GetComponent<MeshExploder> ();
		if (other.name != "laser") {
			StarshipController starship = other.GetComponentInParent<StarshipController> ();
			if (meshExploder != null) {
				meshExploder.Explode ();
				starship.DestroyStarship();
				// starship.OnNetworkDestroy ();
//				if (starship) {
//					GameObject.Destroy(starship.transform.gameObject);
//				}
			}
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
