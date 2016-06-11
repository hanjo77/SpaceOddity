using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class LaserController : MonoBehaviour {

	private AudioSource _audio;
	private bool _isActive;
	private LineRenderer _lineRenderer;
	public StarshipController sourceShip;
	public StarshipController targetShip;

	// Use this for initialization
	void Start () {
		_lineRenderer = gameObject.GetComponent<LineRenderer> ();
		_audio = GetComponent<AudioSource>();
	}
				
	// Update is called once per frame
	void Update () {
		if (_isActive) {			
			_lineRenderer.enabled = false;
			_lineRenderer.SetVertexCount (2);
			_lineRenderer.SetPositions (new Vector3[2] { 
				sourceShip.transform.position,
				sourceShip.laserTarget
			});
			_lineRenderer.enabled = true;
			if (targetShip) {
				targetShip.DecreaseEnergy();
			}
		}
	}

	public void SetLaser(bool doLaser) {
		_isActive = doLaser;
		if (doLaser) {
			if ((sourceShip.isLocalPlayer || (targetShip && targetShip.isLocalPlayer)) && !_audio.isPlaying) {
				_audio.Play();
			}
			gameObject.SetActive (true);
		} else {
			gameObject.SetActive (false);
			_lineRenderer.SetPositions (new Vector3[] {
				sourceShip.transform.position 
			});
		}
	}
}
