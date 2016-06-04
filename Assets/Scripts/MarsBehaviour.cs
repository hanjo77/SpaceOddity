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
		if (other.name != "laser") {
			StarshipController starship = other.GetComponentInParent<StarshipController> ();
			starship.DestroyStarship ();
		}
	}

	// Update is called once per frame
	void Update () {

	}
}
