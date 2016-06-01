using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class LaserController : MonoBehaviour {

	private AudioSource _audio;

	// Use this for initialization
	void Start () {
		_audio = GetComponent<AudioSource>();
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
					targetShip.DestroyStarship();
				}
				if (sourceShip) {
					sourceShip.prefs.ScoreAdd ();
				}
			}
		}
	}
		
	// Update is called once per frame
	void Update () {
		_audio.Play();
		_audio.Play(44100);	
	}
}
