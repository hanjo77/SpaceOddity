using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class MenuBehaviour : MonoBehaviour {

	public Slider sliColorHue;
	public InputField playerNameInput;
	private Prefs _prefs;

	// Use this for initialization
	void Start () {
		_prefs = new Prefs();
		_prefs.Load();
		sliColorHue.value = _prefs.colorHue*(sliColorHue.maxValue+1);
		playerNameInput.text = _prefs.playerName;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnNameFieldValueChange(string name) {
		if (name != "") {
			_prefs.playerName = name;
			_prefs.Save ();
		}
	}

	public void OnSliderChangedColorHue(float colorHue)
	{ 
		GameObject starship = GameObject.FindGameObjectWithTag ("TitleShip");
		_prefs.colorHue = colorHue / (sliColorHue.maxValue+1);
		_prefs.SetStarshipColor(ref starship);
		_prefs.Save ();
	}

	void OnApplicationQuit()
	{ 
		_prefs.Save();
	}
}

