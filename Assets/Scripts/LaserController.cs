using UnityEngine;
using System.Collections;

public class LaserController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	void OnTriggerEnter(Collider other) {
		MeshExploder meshExploder = other.transform.GetComponent<MeshExploder> ();
		if (other.name != "laser") {
			StarshipController sourceShip = transform.parent.GetComponent<StarshipController> ();
			StarshipController targetShip = other.GetComponentInParent<StarshipController> ();
			if (meshExploder != null && !this.transform.IsChildOf(other.transform)) {
				meshExploder.Explode ();
				if (targetShip) {
					targetShip.prefs.ScoreReduce ();
					GameObject.Destroy(targetShip.gameObject);
				}
				if (sourceShip) {
					sourceShip.prefs.ScoreAdd ();
				}
			}
		}
	}
		
	// Update is called once per frame
	void Update () {
	
	}
}
