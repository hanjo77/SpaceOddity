using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class LaserController : MonoBehaviour {

	private AudioSource _audio;
	public float energyDecrease = 1;

	// Use this for initialization
	void Start () {
		_audio = GetComponent<AudioSource>();
		_audio.Play();
	}

	void OnTriggerEnter(Collider other) {
		if (other && other.name != "laser") {
			StarshipController sourceShip = transform.parent.GetComponent<StarshipController> ();
			StarshipController targetShip = other.GetComponentInParent<StarshipController> ();
			if (targetShip) {
				targetShip.energy -= energyDecrease;
				if (!targetShip.isDestroying && targetShip.energy < 0 && !this.transform.IsChildOf(other.transform)) {
					targetShip.energy = 0;
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
	}
		
	// Update is called once per frame
	void Update () {
	}
}
