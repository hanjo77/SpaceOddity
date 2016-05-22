using UnityEngine;
using System.Collections;
using System.Text;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Utility;

public class StarshipController : NetworkBehaviour
{
	//speed stuff
	[SyncVar]float speed;
	[SyncVar]Vector3 motion;
	public int cruiseSpeed;
	float deltaSpeed;//(speed - cruisespeed)
	public int minSpeed;
	public int maxSpeed;
	float accel, decel;
	private Prefs _prefs;
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
	}

	public override void OnStartLocalPlayer ()
	{
		base.OnStartLocalPlayer ();
		Prefs prefs = new Prefs();
		prefs.Load ();
		CmdSyncPrefs(prefs);
		if (!isLocalPlayer) return;
		Transform camera = GameObject.Find("camera").transform;
		CameraFollow follow = camera.GetComponent<CameraFollow> ();
		follow.targetShip = gameObject;
	}

	void Update()
	{
		Debug.Log ("Update starship");
		// Set ship translation, rotation and draw laser

		SetTranslation(motion);
		CmdSetTranslation(motion);
		SetRotation (shipRot);
		CmdSetRotation (shipRot);
		SetLaser(laserActive);
		CmdSetLaser(laserActive);

		if (!isLocalPlayer) return;

		// HandleControls ();

		//ANGULAR DYNAMICS//
		shipRot = transform.localEulerAngles; // I don't know how they're numbered in general.

		//since angles are only stored (0,360), convert to +- 180
		if (shipRot.x > 180) shipRot.x -= 360;
		if (shipRot.y > 180) shipRot.y -= 360;
		if (shipRot.z > 180) shipRot.z -= 360;

		laserActive = Input.GetButton ("Fire1");

		//vertical stick adds to the pitch velocity
		//         (*************************** this *******************************) is a nice way to get the square without losing the sign of the value
		Debug.Log(Input.GetAxis("Vertical"));
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
		else if (Input.GetKey(KeyCode.Joystick1Button0) || Input.GetKey(KeyCode.X))
			speed -= decel * Time.fixedDeltaTime;

		//if not accelerating or decelerating, tend toward cruise, using a similar principle to the accelerations above
		//(added clamping since it's more of a gradual slowdown/speedup) 
		else if (Mathf.Abs(deltaSpeed) > .1f)
			speed -= Mathf.Clamp(deltaSpeed * Mathf.Abs(deltaSpeed), -30, 100) * Time.fixedDeltaTime;
		

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

		Debug.Log (shipRot.ToString ());
		float sqrOffset = transform.localPosition.normalized.sqrMagnitude;
		Vector3 offsetDir = transform.localPosition.normalized;
		motion = (transform.forward * speed) * Time.fixedDeltaTime;


//		_camera.localPosition = cameraOffset + new Vector3(0, 0, -deltaSpeed * .02f);
		this.ShowClosestNeighbor ();
	}

	void OnColorChanged(Color c) {
		GameObject go = gameObject;
		Renderer renderer = gameObject.GetComponentsInChildren<Renderer> () [0];
		renderer.material.SetColor("_Color", c);
	}

	public override void OnNetworkDestroy() {
		base.OnNetworkDestroy ();
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
		Transform wingTransform = transform.Find("colorparts");
		wingTransform.GetComponent<MeshRenderer>().material.color = clr; 
	}

	void SetTranslation(Vector3 trans) {
		Debug.Log (trans);
		transform.Translate (trans, Space.World);
	}

	void SetRotation(Vector3 rotation) {
		Debug.Log (rotation);
		transform.Rotate (rotation);
	}

	void SetLaser(bool doLaser)
	{
		Transform laser = transform.Find ("laser");
		if (doLaser) {
			laser.localScale = new Vector3 (.1f, .1f, 1000);
			laser.localPosition = new Vector3 (0, 0, 500);
		} else {
			laser.localScale = new Vector3 (.1f, .1f, .1f);
			laser.localPosition = new Vector3 (0, 0, 0);
		}
	}
		
	////////////////////////////
	// Network Syncronisation //
	////////////////////////////
	// Command functions all called on clients and executed on the server
	[Command] void CmdSetColor (Color c) { 
		SetColor(c); 
	}
	[Command] void CmdSetLaser (bool l) { 
		SetLaser(l); 
	}
	[Command] void CmdSyncPrefs (Prefs p) { 
		_prefs = p; 
	}
	[Command] void CmdSetTranslation (Vector3 s) {
		SetTranslation (s);
	}
	[Command] void CmdSetRotation (Vector3 r) {
		SetRotation (r);
	}

	// [Command] void CmdSpawn(GameObject g) { NetworkServer.Spawn(g); }
	[Command]
	void CmdSpawn()
	{
		NetworkServer.SpawnWithClientAuthority(gameObject, connectionToClient);
	}
}

