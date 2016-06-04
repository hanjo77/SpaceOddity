using UnityEngine;
using System.Collections;
using System.Text;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Utility;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(AudioSource))]
public class StarshipController : NetworkBehaviour
{
	//speed stuff
	[SyncVar]float speed;
	[SyncVar]Vector3 motion;
	public int cruiseSpeed;
	float deltaSpeed;//(speed - cruisespeed)
	public int minSpeed;
	public int maxSpeed;
	public float maxArrowScale = 1;
	float accel, decel;
	[SyncVar(hook = "OnPrefsChanged")]private Prefs _prefs = new Prefs();
	public Transform arrow;
	[SyncVar]public ParticleSystem explosion;
	private Transform _arrow;
	[SyncVar]public bool isDestroying = false;
	private IEnumerator _waitCoroutine;
	public float arrowDistance = 5f;
	public Prefs prefs { get { return _prefs; } }

	//turning stuff
	[SyncVar]Vector3 angVel;
	[SyncVar]Vector3 shipRot;
	public int sensitivity;

	//laser
	[SyncVar(hook = "OnLaserChanged")]bool laserActive;
	private bool _boost;

	public Vector3 cameraOffset; //I use (0,1,-3)

	public Vector2 touchOrigin = -Vector2.one;

	private float _maxEnergy = 1;
	[SyncVar]public float energy;

	//audio
	[SyncVar]private AudioSource _engineAudioSource;
	public AudioClip laserSound; 
	public AudioClip explosionSound; 

	void Start() {
		SetSpeed (cruiseSpeed);
		SetEnergy (_maxEnergy);
		ReapplyPrefs ();
	}

	public override void OnStartClient()
	{
		Start ();
	}

	public override void OnStartServer()
	{
		Start ();
	}

	public override void OnStartLocalPlayer ()
	{
		base.OnStartLocalPlayer ();
		GameObject energyBarObject = GameObject.FindGameObjectWithTag ("EnergyBar");
		if (energyBarObject) 
		{
			EnergyBarBehaviour energyBar = energyBarObject.GetComponent<EnergyBarBehaviour>();
			energyBar.starShip = this;
		}
		CmdStartEngineSound ();
		_prefs.Load ();
		CmdSyncPrefs (_prefs);
		ReapplyPrefs ();
		TrackCameraTo (gameObject);
	}
		
	void Update()
	{
		Debug.Log ("Update starship");
		// Set ship translation, rotation and draw laser

		if (isDestroying) {
			if (_engineAudioSource && _engineAudioSource.isPlaying) _engineAudioSource.Stop ();
			return;
		}

		SetTranslation(motion);
		SetRotation (shipRot);
		SetLaser(laserActive);
		SetSpeed (speed);

		if (!isLocalPlayer) return;

		//ANGULAR DYNAMICS//
		shipRot = transform.localEulerAngles; // I don't know how they're numbered in general.

		//since angles are only stored (0,360), convert to +- 180
		if (shipRot.x > 180) shipRot.x -= 360;
		if (shipRot.y > 180) shipRot.y -= 360;
        if (shipRot.z > 180) shipRot.z -= 360;

        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            laserActive = Input.GetButton("Fire1");

            //vertical stick adds to the pitch velocity
            //         (*************************** this *******************************) is a nice way to get the square without losing the sign of the value
            angVel.x += Input.GetAxis("Vertical") * Mathf.Abs(Input.GetAxis("Vertical")) * sensitivity * Time.fixedDeltaTime;

            //horizontal stick adds to the roll and yaw velocity... also thanks to the .5 you can't turn as fast/far sideways as you can pull up/down
            float turn = Input.GetAxis("Horizontal") * Mathf.Abs(Input.GetAxis("Horizontal")) * sensitivity * Time.fixedDeltaTime;
            angVel.y += turn * .5f;
            angVel.z -= turn * .5f;


            //shoulder buttons add to the roll and yaw.  No deltatime here for a quick response
            //comment out the .y parts if you don't want to turn when you hit them
            if (Input.GetKey(KeyCode.Joystick1Button4) || Input.GetKey(KeyCode.I))
            {
                angVel.y -= 20;
                angVel.z += 50;
                speed -= 5 * Time.fixedDeltaTime;
            }

            if (Input.GetKey(KeyCode.Joystick1Button5) || Input.GetKey(KeyCode.O))
            {
                angVel.y += 20;
                angVel.z -= 50;
                speed -= 5 * Time.fixedDeltaTime;
            }
        }
        else if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            Debug.Log(CrossPlatformInputManager.GetAxis("Vertical"));

            angVel.x += CrossPlatformInputManager.GetAxis("Vertical") * Mathf.Abs(CrossPlatformInputManager.GetAxis("Vertical")) * sensitivity * Time.fixedDeltaTime;
            float turn = CrossPlatformInputManager.GetAxis("Horizontal") * Mathf.Abs(CrossPlatformInputManager.GetAxis("Horizontal")) * sensitivity * Time.fixedDeltaTime;
            angVel.y += turn;
            angVel.z -= turn;
            _boost = CrossPlatformInputManager.GetButton("Boost");
            laserActive = CrossPlatformInputManager.GetButton("Shoot");

        }
		//simple accelerations
		if (_boost || Input.GetKey(KeyCode.Joystick1Button1) || Input.GetKey(KeyCode.RightShift))
			speed += accel * Time.fixedDeltaTime;
		else if (Input.GetKey(KeyCode.Joystick1Button0) || Input.GetKey(KeyCode.X))
			speed -= decel * Time.fixedDeltaTime;

		//if not accelerating or decelerating, tend toward cruise, using a similar principle to the accelerations above
		//(added clamping since it's more of a gradual slowdown/speedup) 
		else if (Mathf.Abs(deltaSpeed) > .1f)
			speed -= Mathf.Clamp(deltaSpeed * Mathf.Abs(deltaSpeed), -30, 100) * Time.fixedDeltaTime;

		Debug.Log(CrossPlatformInputManager.GetAxis("Vertical"));

		//your angular velocity is higher when going slower, and vice versa.  There probably exists a better function for this.
		angVel /= 1 + deltaSpeed * .001f;

		//this is what limits your angular velocity.  Basically hard limits it at some value due to the square magnitude, you can
		//tweak where that value is based on the coefficient
		angVel -= angVel.normalized * angVel.sqrMagnitude * .08f * Time.fixedDeltaTime;


		//LINEAR DYNAMICS//

		deltaSpeed = speed - cruiseSpeed;

		//This, I think, is a nice way of limiting your speed.  Your acceleration goes to zero as you approach the min/max speeds, and you initially
		//brake and accelerate a lot faster.  Could potentially do the same thing with the angular stuff.
		decel = speed - minSpeed;
		accel = maxSpeed - speed;

		shipRot = new Vector3(
			shipRot.x * Time.fixedDeltaTime, 
			(shipRot.y * Mathf.Abs(shipRot.y) * .02f) * Time.fixedDeltaTime, 
			shipRot.z * Time.fixedDeltaTime);
		shipRot = (angVel * Time.fixedDeltaTime);
		//
		//this limits your rotation, as well as gradually realigns you.  It's a little convoluted, but it's
		//got the same square magnitude functionality as the angular velocity, plus a constant since x^2
		//is very small when x is small.  Also realigns faster based on speed.  feel free to tweak
		//		shipRot = -shipRot.normalized * .015f * (shipRot.sqrMagnitude + 500) * (1 + speed / maxSpeed) * Time.fixedDeltaTime;

		motion = (transform.forward * speed) * Time.fixedDeltaTime;
		this.ShowClosestNeighbor ();
		CmdSetTranslation(motion);
		CmdSetRotation (shipRot);
		CmdSetLaser(laserActive);
		CmdSetSpeed (speed);
	}

	void OnColorChanged(Color c) {
		Renderer renderer = gameObject.GetComponentsInChildren<Renderer> () [0];
		renderer.material.SetColor("_Color", c);
	}

	public void Respawn()
	{
		// StopCoroutine(_waitCoroutine);
		StopAllCoroutines();
		// gameObject.SetActive (true);
		SetEnergy(_maxEnergy);
		ParticleSystem[] explosions = FindObjectsOfType<ParticleSystem> ();
		foreach (ParticleSystem p in explosions) {
			GameObject.Destroy (p.gameObject);
		}
		NetworkStartPosition[] spawnPoints = FindObjectsOfType<NetworkStartPosition> ();
		Vector3 spawnPoint = spawnPoints [Random.Range (0, spawnPoints.Length)].transform.position;
		gameObject.transform.position = spawnPoint;
		StartEngineSound ();
		isDestroying = false;
	}

	public void TrackCameraTo(GameObject go) 
	{
		GameObject camera = GameObject.Find ("camera");
		if (isLocalPlayer && camera != null)
		{
			CameraFollow follow = camera.transform.GetComponent<CameraFollow> ();
			follow.targetShip = go;
		}
	}

	void OnLaserChanged(bool l) {
		laserActive = l;
		SetLaser (l);
	}

	public void DestroyStarship() {
		isDestroying = true;
		if (isLocalPlayer) {
			StartExplosion ();
		} else {
			CmdStartExplosion ();
		}
//		Respawn ();
	}

	void OnDestroy() {
		Respawn ();
	}

	public override void OnNetworkDestroy() {
		base.OnNetworkDestroy ();
		OnDestroy();
	}

	private void ShowClosestNeighbor() 
	{
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		float minDist = int.MaxValue;
		GameObject myPlayer = this.gameObject;
		GameObject closestPlayer = null;
		foreach (GameObject player in players) {
			if (player != myPlayer) {
				float dist = Vector3.Distance (player.transform.position, myPlayer.transform.position);
				if (dist < minDist && player.GetComponent<StarshipController> ()) {
					minDist = dist;
					closestPlayer = player;
				}
			}
		}
		if (closestPlayer != null) {
			if (_arrow == null) {
				_arrow = Instantiate (arrow) as Transform;
				_arrow.localPosition = new Vector3 (0, 0, 0);
				_arrow.transform.parent = transform;
			}
			Prefs otherPrefs = closestPlayer.GetComponent<StarshipController> ().prefs;
			Renderer renderer = _arrow.transform.GetComponentsInChildren<Renderer> () [0];
			renderer.material.SetColor("_Color", Color.HSVToRGB(otherPrefs.colorHue, otherPrefs.colorSaturation, otherPrefs.colorLuminance));
			_arrow.LookAt (closestPlayer.transform.position);
			_arrow.localScale = GetArrowScale (myPlayer, closestPlayer);
			_arrow.transform.position = Vector3.MoveTowards (myPlayer.transform.position, closestPlayer.transform.position, arrowDistance);
		}
		else if (_arrow != null) {
			GameObject.Destroy (_arrow.gameObject);
			_arrow = null;
		}
	}

	Vector3 GetArrowScale(GameObject myPlayer, GameObject otherPlayer)
	{
		Vector3 pos1 = myPlayer.transform.position;
		Vector3 pos2 = otherPlayer.transform.position;
		float dist = Vector3.Distance (pos1, pos2);
		if (dist < 2 * arrowDistance) {
			return new Vector3(0, 0, 0);
		} else {
			float scale = dist - (2 * arrowDistance);
			scale = (scale < maxArrowScale ? scale : maxArrowScale);
			return new Vector3 (scale, scale, scale);
		}
	}

	void SetTranslation(Vector3 trans) {
		Debug.Log (trans);
		transform.Translate (trans, Space.World);
	}

	void SetRotation(Vector3 rotation) {
		Debug.Log (rotation);
		transform.Rotate (rotation);
	}

	public void SetBoost(bool doBoost)
	{
		_boost = doBoost;
	}

	public void SetSpeed(float s)
	{
		speed = s;
		if (_engineAudioSource) {
			_engineAudioSource.pitch = s / 100;
		}
	}

	public void SetEnergy(float e)
	{
		energy = e;
	}

	public void SetName(string name)
	{
		this.name = name;
	}

	public void Explode()
	{
		var meshExploders = transform.GetComponentsInChildren<MeshExploder> ();
		foreach (MeshExploder meshExploder in meshExploders) {
			meshExploder.Explode ();
		}
	}

	public IEnumerator Wait(float waitTime) {
		while (true) {
			yield return new WaitForSeconds(waitTime);
			Respawn ();
		}
	}

	public void SetLaser(bool doLaser)
	{
		Transform laser = transform.Find ("laser");
		if (doLaser) {
			laser.gameObject.SetActive (true);
			laser.localScale = new Vector3 (.1f, .1f, 1000);
			laser.localPosition = new Vector3 (0, 0, 500);
		} else {
			laser.gameObject.SetActive (false);
			laser.localScale = new Vector3 (.1f, .1f, .1f);
			laser.localPosition = new Vector3 (0, 0, 0);
		}
	}

	void StartEngineSound()
	{
		if (isLocalPlayer) 
		{
			_engineAudioSource = GetComponent<AudioSource>();
			_engineAudioSource.Play();
		}
	}

	void StartExplosion()
	{
		ParticleSystem p = ((GameObject)Instantiate (explosion.gameObject, transform.position, transform.rotation)).GetComponent<ParticleSystem>();
		p.transform.parent = transform;
		p.Play ();
		AudioSource.PlayClipAtPoint (explosionSound, transform.position, 100);
		Explode ();
		if (!isLocalPlayer)
			return;
		_waitCoroutine = Wait(p.duration);
		StartCoroutine(_waitCoroutine);
	}

	void OnPrefsChanged(Prefs prefs)
	{
		_prefs = prefs;
		ReapplyPrefs();
	}

	public void ReapplyPrefs() {
		 GameObject go = gameObject;
		 prefs.SetAll(ref go);
	}
	
		
	////////////////////////////
	// Network Syncronisation //
	////////////////////////////
	// Command functions all called on clients and executed on the server
	[Command] void CmdSetSpeed (float s) { 
		SetSpeed(s); 
	}
	[Command] void CmdSetEnergy (float e) { 
		SetSpeed(e); 
	}
	[Command] void CmdSetName (string name) { 
		SetName(name); 
	}
	[Command] void CmdSetLaser (bool l) { 
		laserActive = l; 
	}
	[Command] void CmdSyncPrefs (Prefs p) { 
		_prefs = p; 
		ReapplyPrefs ();
	}
	[Command] void CmdSetTranslation (Vector3 s) {
		SetTranslation (s);
	}
	[Command] void CmdSetRotation (Vector3 r) {
		SetRotation (r);
	}

	[Command]
	void CmdSpawn()
	{
		var go = (GameObject)Instantiate(
			gameObject, 
			transform.position + new Vector3(0,1,0), 
			Quaternion.identity);

		NetworkServer.SpawnWithClientAuthority(go, connectionToClient);
	}

	[Command]
	void CmdStartExplosion()
	{
		StartExplosion ();
	}

	[Command]
	void CmdStartEngineSound()
	{
		StartEngineSound ();
	}

	[Command]
	void CmdDestroyStarship() {
		DestroyStarship ();
	}


	[ClientRpc]
	void RpcGameOver()
	{ 
		GameObject.Destroy(this.gameObject);
		NetworkManager.singleton.ServerChangeScene ("Start");
	}
}

