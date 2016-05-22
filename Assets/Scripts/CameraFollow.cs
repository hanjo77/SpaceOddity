using UnityEngine;
using System.Collections.Generic;

public class CameraFollow : MonoBehaviour
{
	public GameObject targetShip;
	public float distance;
	public float height;
	public float damping;
	public float rotationDamping;
	public bool smoothRotation;
	// Use this for initialization
	void Start () 
	{
	}
	void Update () 
	{
		if (targetShip) {
			Transform target = targetShip.transform;
			Vector3 wantedPosition = target.TransformPoint(0, height, -distance);
			transform.position = Vector3.Lerp (transform.position, wantedPosition, Time.deltaTime * damping);
			if (smoothRotation) {
				var wantedRotation = Quaternion.LookRotation(target.position - transform.position, target.up);
				transform.rotation = Quaternion.Slerp (transform.rotation, wantedRotation, Time.deltaTime * rotationDamping);
			}
			else transform.LookAt (target, target.up);
		}
	}
}