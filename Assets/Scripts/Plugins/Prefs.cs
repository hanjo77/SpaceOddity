﻿using UnityEngine;

public class Prefs 
{
	public float colorHue;
	public float colorSaturation;
	public float colorLuminance;
	public string playerName;

	public void Load()
	{
		colorHue = PlayerPrefs.GetFloat("colorHue", 1f);
		colorSaturation = PlayerPrefs.GetFloat("colorSaturation", 1f);
		colorLuminance = PlayerPrefs.GetFloat("colorLuminance", 1f);
		playerName = PlayerPrefs.GetString("playerName");
	}

	public void Save()
	{
		PlayerPrefs.SetFloat("colorHue", colorHue);
		PlayerPrefs.SetFloat("colorSaturation", colorSaturation);
		PlayerPrefs.SetFloat("colorLuminance", colorLuminance);
		PlayerPrefs.SetString("playerName", playerName);
	}

	public void SetAll(ref GameObject arWing)
	{
		SetArwingColor (ref arWing);
	}

	public void SetArwingColor(ref GameObject arwing) {

		try {
			GameObject gameObject = arwing.transform.Find("colorparts").gameObject;
			Renderer renderer = gameObject.GetComponentsInChildren<Renderer> () [0];
			renderer.material.SetColor("_Color", Color.HSVToRGB(colorHue, colorSaturation, colorLuminance));
		}
		finally {
		}
	}
}