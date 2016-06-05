using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class LaserController : MonoBehaviour {

	private AudioSource _audio;
	public float energyDecrease = .1f;

	// Use this for initialization
	void Start () {
		_audio = GetComponent<AudioSource>();
		_audio.Play();
	}

	void OnTriggerEnter(Collider other) {
		if (other && other.name != "laser") {
			StarshipController sourceShip = transform.parent.GetComponent<StarshipController> ();
			StarshipController targetShip = other.GetComponentInParent<StarshipController> ();
			if (targetShip && !this.transform.IsChildOf(other.transform)) {
				targetShip.DecreaseEnergy (energyDecrease, sourceShip);
			}
		}
	}
		
	// Update is called once per frame
	void Update () {
	}
}
