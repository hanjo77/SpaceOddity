using UnityEngine;
using System.Collections;

public class LaserController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	void OnTriggerEnter(Collider other) {
		MeshExploder meshExploder = other.transform.GetComponent<MeshExploder> ();
		if (other.name != "laser") {
			StarshipController starship = other.GetComponentInParent<StarshipController> ();
			if (meshExploder != null && !this.transform.IsChildOf(other.transform)) {
				meshExploder.Explode ();
				if (starship) {
					GameObject.Destroy(starship.gameObject);
				}
			}
		}
	}
		
	// Update is called once per frame
	void Update () {
	
	}
}
