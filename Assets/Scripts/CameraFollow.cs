using UnityEngine;
using UnityStandardAssets.Utility;

public class CameraFollow : SmoothFollow
{
	// Use this for initialization
	void Start() {
	}

	public void SetTarget(Transform target) {
		base.target = target;
	}
}