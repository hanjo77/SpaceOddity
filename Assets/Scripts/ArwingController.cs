﻿using UnityEngine;
using System.Collections;
using System.Text;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class ArwingController : NetworkBehaviour
{
	//speed stuff
	[SyncVar]float speed;
	public int cruiseSpeed;
	float deltaSpeed;//(speed - cruisespeed)
	public int minSpeed;
	public int maxSpeed;
	float accel, decel;
	private Prefs _prefs;
	private Transform _camera;
	public Transform arrow;
	private Transform _arrow;
	public float arrowDistance = 5f;

	[SyncVar(hook = "OnColorChanged")]Color color = Color.red;

	//turning stuff
	[SyncVar]Vector3 angVel;
	[SyncVar]Vector3 shipRot;
	public int sensitivity;

	//laser
	[SyncVar]public bool laserActive;

	public Vector3 cameraOffset; //I use (0,1,-3)

	void Start() {
		speed = cruiseSpeed;
		_prefs = new Prefs();
		_prefs.Load();
		color = Color.HSVToRGB (_prefs.colorHue, _prefs.colorSaturation, _prefs.colorLuminance);
		SetColor (color);
		CmdSetColor (color);
		if (!isLocalPlayer)
			return;
	}

	public override void OnStartLocalPlayer ()
	{
		base.OnStartLocalPlayer ();
		Debug.Log (isLocalPlayer);
		// CmdSpawn (gameObject);
		Prefs prefs = new Prefs();
		prefs.Load ();
		CmdSyncPrefs(prefs);
		if (!isLocalPlayer) return;
		if (!transform.Find("camera")) {
			_camera = GameObject.Find("camera").transform;
			_camera.position = new Vector3 (0, 0, 0);
			_camera.localPosition = new Vector3 (0, 0, 0);
			_camera.parent = transform;
		}
	}

	void FixedUpdate()
	{
		Debug.Log ("Update arwing");
		// Set ship translation, rotation and draw laser

		SetTranslation(speed);
		SetRotation (shipRot, angVel);
		SetLaser();

		if (!isLocalPlayer) return;

		// HandleControls ();

		laserActive = Input.GetButton ("Fire1");

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

		//simple accelerations
		if (Input.GetKey(KeyCode.Joystick1Button1) || Input.GetKey(KeyCode.LeftShift))
			speed += accel * Time.fixedDeltaTime;
		else if (Input.GetKey(KeyCode.Joystick1Button0) || Input.GetKey(KeyCode.Space))
			speed -= decel * Time.fixedDeltaTime;

		//if not accelerating or decelerating, tend toward cruise, using a similar principle to the accelerations above
		//(added clamping since it's more of a gradual slowdown/speedup) 
		else if (Mathf.Abs(deltaSpeed) > .1f)
			speed -= Mathf.Clamp(deltaSpeed * Mathf.Abs(deltaSpeed), -30, 100) * Time.fixedDeltaTime;
		

		//ANGULAR DYNAMICS//
		shipRot = transform.Find("arwing").localEulerAngles; //make sure you're getting the right child (the ship).  I don't know how they're numbered in general.

		//since angles are only stored (0,360), convert to +- 180
		if (shipRot.x > 180) shipRot.x -= 360;
		if (shipRot.y > 180) shipRot.y -= 360;
		if (shipRot.z > 180) shipRot.z -= 360;

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

		_camera.localPosition = cameraOffset + new Vector3(0, 0, -deltaSpeed * .02f);
		this.ShowClosestNeighbor ();
		Debug.Log(cameraOffset);
	}

	void OnColorChanged(Color c) {
		// _prefs.SetArwingColor (arWing);
		Renderer renderer = gameObject.GetComponentsInChildren<Renderer> () [0];
		renderer.material.SetColor("_Color", c);
	}

	public override void OnNetworkDestroy() {
		if (!isLocalPlayer)
			return;
		base.OnNetworkDestroy ();
		try {
			_camera.SetParent (this.transform.root);
			_camera.position = this.transform.position;
			_camera.rotation = this.transform.rotation;
		}
		finally {
		}
		NetworkManager.singleton.ServerChangeScene ("Start");
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
				if (dist < minDist) {
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
			_arrow.LookAt (closestPlayer.transform.position);
			_arrow.transform.position = Vector3.MoveTowards (myPlayer.transform.position, closestPlayer.transform.position, arrowDistance);
		}
		else if (_arrow != null) {
			GameObject.Destroy (_arrow.gameObject);
			_arrow = null;
		}
	}

	void SetColor(Color clr)
	{       
		Transform wingTransform = transform.Find("arwing/colorparts");
		wingTransform.GetComponent<MeshRenderer>().material.color = clr; 
	}

	void SetTranslation(float speed) {
		float sqrOffset = transform.localPosition.normalized.sqrMagnitude;
		Vector3 offsetDir = transform.localPosition.normalized;
		transform.Translate((offsetDir * sqrOffset * 50 + transform.forward * speed) * Time.fixedDeltaTime, Space.World);
	}

	void SetRotation(Vector3 rotation, Vector3 angularVelocity) {
		transform.Rotate(rotation.x * Time.fixedDeltaTime, (rotation.y * Mathf.Abs(rotation.y) * .02f) * Time.fixedDeltaTime, rotation.z * Time.fixedDeltaTime);
		transform.Find("arwing").Rotate(angularVelocity * Time.fixedDeltaTime);

		//this limits your rotation, as well as gradually realigns you.  It's a little convoluted, but it's
		//got the same square magnitude functionality as the angular velocity, plus a constant since x^2
		//is very small when x is small.  Also realigns faster based on speed.  feel free to tweak
		transform.Find("arwing").Rotate(-rotation.normalized * .015f * (rotation.sqrMagnitude + 500) * (1 + speed / maxSpeed) * Time.fixedDeltaTime);
	}

	void SetLaser()
	{
		Transform laser = transform.Find ("arwing/laser");
		if (laserActive) {
			laser.localScale = new Vector3 (.1f, .1f, 1000);
			laser.localPosition = new Vector3 (0, 0, 500);
		} else {
			laser.localScale = new Vector3 (.1f, .1f, .1f);
		}
	}
		
	////////////////////////////
	// Network Syncronisation //
	////////////////////////////
	// Command functions all called on clients and executed on the server
	[Command] void CmdSetColor (Color c) { SetColor(c); }
	[Command] void CmdSyncPrefs (Prefs p) { _prefs = p; }
	// [Command] void CmdSpawn(GameObject g) { NetworkServer.Spawn(g); }
	[Command]
	void CmdSpawn()
	{
		NetworkServer.SpawnWithClientAuthority(gameObject, connectionToClient);
	}
}

