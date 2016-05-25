using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class MenuBehaviour : MonoBehaviour {

	public Slider sliColorHue;
	public GameObject starship;
	public InputField playerNameInput;
	public GameObject inputFieldContainer;
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

	public void ToggleInputFields() {
		inputFieldContainer.SetActive (!inputFieldContainer.activeInHierarchy);
	}

	public void OnNameFieldValueChange(string name) {
		_prefs.playerName = name;
	}

	public void OnSliderChangedColorHue(float colorHue)
	{ 
		_prefs.colorHue = colorHue / (sliColorHue.maxValue+1);
		_prefs.SetStarshipColor(ref starship);
		_prefs.Save ();
	}

	void OnApplicationQuit()
	{ 
		_prefs.Save();
		Debug.Log (_prefs.playerName);
	}
}

