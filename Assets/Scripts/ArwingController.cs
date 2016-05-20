﻿using UnityEngine;
using System.Collections;
using System.Text;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class ArwingController : NetworkBehaviour
{
	//speed stuff
	float speed;
	public int cruiseSpeed;
	float deltaSpeed;//(speed - cruisespeed)
	public int minSpeed;
	public int maxSpeed;
	public Transform arWing;
	float accel, decel;
	private Prefs _prefs;
	private Transform _camera;
	public Transform arrow;
	private Transform _arrow;
	public float arrowDistance = 5f;
	private int _scene;
	private Transform _laser;
	public bool laserActive;

	public Color           color = Color.red;

	//turning stuff
	Vector3 angVel;
	Vector3 shipRot;
	public int sensitivity;

	public Vector3 cameraOffset; //I use (0,1,-3)
	public const int DefaultReliable = 0;

	void Awake() {
		_scene = SceneManager.GetActiveScene().buildIndex;
		Debug.Log (_scene);
		speed = cruiseSpeed;
		_camera = GameObject.Find("camera").transform;
		_camera.position = new Vector3 (0, 0, 0);
		_camera.localPosition = new Vector3 (0, 0, 0);
		_camera.parent = arWing;
		_prefs = new Prefs();
		_prefs.Load();
		color = Color.HSVToRGB (_prefs.colorHue, _prefs.colorSaturation, _prefs.colorLuminance);
		ChangeColor (color);
		// CmdChangeColor (color);
		_laser = arWing.Find("arwing/laser");
		_camera = GameObject.Find ("camera").transform;
		if (!isLocalPlayer)
			return;
	}

	void FixedUpdate()
	{
		Debug.Log ("Update arwing");
		// if (!isLocalPlayer) return;
		if (!transform.Find("camera")) {
			_camera.parent = arWing;
		}
		FireLaser();
		laserActive = Input.GetButton ("Fire1");
		//ANGULAR DYNAMICS//
		shipRot = arWing.Find("arwing").localEulerAngles; //make sure you're getting the right child (the ship).  I don't know how they're numbered in general.

		//since angles are only stored (0,360), convert to +- 180
		if (shipRot.x > 180) shipRot.x -= 360;
		if (shipRot.y > 180) shipRot.y -= 360;
		if (shipRot.z > 180) shipRot.z -= 360;

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


		//your angular velocity is higher when going slower, and vice versa.  There probably exists a better function for this.
		angVel /= 1 + deltaSpeed * .001f;

		//this is what limits your angular velocity.  Basically hard limits it at some value due to the square magnitude, you can
		//tweak where that value is based on the coefficient
		angVel -= angVel.normalized * angVel.sqrMagnitude * .08f * Time.fixedDeltaTime;


		//and finally rotate.  
		arWing.Find("arwing").Rotate(angVel * Time.fixedDeltaTime);

		//this limits your rotation, as well as gradually realigns you.  It's a little convoluted, but it's
		//got the same square magnitude functionality as the angular velocity, plus a constant since x^2
		//is very small when x is small.  Also realigns faster based on speed.  feel free to tweak
		arWing.Find("arwing").Rotate(-shipRot.normalized * .015f * (shipRot.sqrMagnitude + 500) * (1 + speed / maxSpeed) * Time.fixedDeltaTime);


		//LINEAR DYNAMICS//

		deltaSpeed = speed - cruiseSpeed;

		//This, I think, is a nice way of limiting your speed.  Your acceleration goes to zero as you approach the min/max speeds, and you initially
		//brake and accelerate a lot faster.  Could potentially do the same thing with the angular stuff.
		decel = speed - minSpeed;
		accel = maxSpeed - speed;

		//simple accelerations
		if (Input.GetKey(KeyCode.Joystick1Button1) || Input.GetKey(KeyCode.LeftShift))
			speed += accel * Time.fixedDeltaTime;
		else if (Input.GetKey(KeyCode.Joystick1Button0) || Input.GetKey(KeyCode.Space))
			speed -= decel * Time.fixedDeltaTime;

		//if not accelerating or decelerating, tend toward cruise, using a similar principle to the accelerations above
		//(added clamping since it's more of a gradual slowdown/speedup) 
		else if (Mathf.Abs(deltaSpeed) > .1f)
			speed -= Mathf.Clamp(deltaSpeed * Mathf.Abs(deltaSpeed), -30, 100) * Time.fixedDeltaTime;


		float sqrOffset = arWing.localPosition.normalized.sqrMagnitude;
		Vector3 offsetDir = arWing.localPosition.normalized;


		//this takes care of realigning after collisions, where the ship gets displaced due to its rigidbody.
		//I'm pretty sure this is the best way to do it (have the ship and the rig move toward their mutual center)
		transform.Translate(-offsetDir * sqrOffset * 20 * Time.fixedDeltaTime);
		//(**************** this ***************) is what actually makes the whole ship move through the world!
		arWing.Translate((offsetDir * sqrOffset * 50 + arWing.forward * speed) * Time.fixedDeltaTime, Space.World);

		//comment this out for starfox, remove the x and z components for shadows of the empire, and leave the whole thing for free roam
		arWing.Rotate(shipRot.x * Time.fixedDeltaTime, (shipRot.y * Mathf.Abs(shipRot.y) * .02f) * Time.fixedDeltaTime, shipRot.z * Time.fixedDeltaTime);
		this.ShowClosestNeighbor ();

		//moves camera (make sure you're GetChild()ing the camera's index)
		//I don't mind directly connecting this to the speed of the ship, because that always changes smoothly
		_camera.localPosition = cameraOffset + new Vector3(0, 0, -deltaSpeed * .02f);
		Debug.Log(cameraOffset);
	}

	public override void OnNetworkDestroy() {
		base.OnNetworkDestroy ();
		if (isLocalPlayer) {
			try {
				_camera.SetParent (this.transform.root);
				_camera.position = this.transform.position;
				_camera.rotation = this.transform.rotation;
			}
			finally {
			}
		}
		SceneManager.LoadScene ("Start");
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

	void ChangeColor(Color clr)
	{       
		Transform wingTransform = transform.Find("arwing/colorparts");
		wingTransform.GetComponent<MeshRenderer>().material.color = clr; 
	}

	void FireLaser()
	{
		if (laserActive) {
			_laser.localScale = new Vector3 (.1f, .1f, 1000);
			_laser.localPosition = new Vector3 (0, 0, 500);
		} else {
			_laser.localScale = new Vector3 (.1f, .1f, .1f);
		}
	}
		
//	[Command]
//	void CmdChangeColor(Color c) {
//		ChangeColor (c);
//	}

//	[Command]
//	void CmdFireLaser() {
//		FireLaser ();
//	}
}

