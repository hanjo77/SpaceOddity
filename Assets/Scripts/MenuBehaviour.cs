using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class MenuBehaviour : MonoBehaviour {

	public Slider sliColorHue;
	public GameObject arwing;
	private Prefs _prefs;

	// Use this for initialization
	void Start () {
		_prefs = new Prefs();
		_prefs.Load();
		sliColorHue.value = _prefs.colorHue;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnNameFieldValueChange(string name) {
		_prefs.playerName = name;
	}

	public void OnSliderChangedColorHue(float colorHue)
	{ 
		_prefs.colorHue = colorHue;
		_prefs.SetArwingColor(ref arwing);
		_prefs.Save ();
	}

	public void OnButtonPlayClicke() {
		_prefs.Save();
		SceneManager.LoadScene ("Space");
	}

	void OnApplicationQuit()
	{ 
		_prefs.Save();
	}
}

