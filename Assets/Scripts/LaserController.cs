using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LaserController : MonoBehaviour {

	private AudioSource _audio;
	private bool _isActive;
	private LineRenderer _lineRenderer;
	public StarshipController sourceShip;
	public StarshipController targetShip;

	// Use this for initialization
	void Start () {
		if (!_lineRenderer)
			_lineRenderer = gameObject.GetComponent<LineRenderer> ();
		if (!_audio)
			_audio = GetComponent<AudioSource>();
	}
				
	// Update is called once per frame
	void Update () {
		if (_isActive) {			
			_lineRenderer.SetVertexCount (2);
			_lineRenderer.SetPositions (new Vector3[2] { 
				sourceShip.transform.position,
				sourceShip.laserTarget
			});
			if (targetShip) {
				targetShip.DecreaseEnergy();
			}
		}
        else
        {
            _lineRenderer.SetPositions(new Vector3[2] {
                sourceShip.transform.position,
                sourceShip.transform.position
            });
        }
    }

	public void SetLaser(bool doLaser) {
		_isActive = doLaser;
		if (doLaser) {
			gameObject.SetActive (true);
			if ((sourceShip.isLocalPlayer || (targetShip && targetShip.isLocalPlayer))
                && !_audio.isPlaying && _audio.enabled) {
				_audio.Play();
			}
			Update ();
		} else {
			gameObject.SetActive (false);
		}
	}
}
