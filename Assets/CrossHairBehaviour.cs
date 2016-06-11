using UnityEngine;
using System.Collections;

public class CrossHairBehaviour : MonoBehaviour {

	public StarshipController starShip;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

	public Vector3 GetDistance(Transform other) {
		return (other.position - starShip.transform.position);
	}
}
