using UnityEngine;

public class Prefs 
{
	public float colorHue;
	public float colorSaturation;
	public float colorLuminance;
	public string playerName;
	public int score;

	public void Load()
	{
		colorHue = PlayerPrefs.GetFloat("colorHue", 1f);
		colorSaturation = PlayerPrefs.GetFloat("colorSaturation", 1f);
		colorLuminance = PlayerPrefs.GetFloat("colorLuminance", 1f);
		playerName = PlayerPrefs.GetString("playerName");
		score = PlayerPrefs.GetInt ("score");
	}

	public void Save()
	{
		PlayerPrefs.SetFloat("colorHue", colorHue);
		PlayerPrefs.SetFloat("colorSaturation", colorSaturation);
		PlayerPrefs.SetFloat("colorLuminance", colorLuminance);
		PlayerPrefs.SetString("playerName", playerName);
		PlayerPrefs.SetInt ("score", score);
	}

	public void ScoreAdd() {
		score++;
	}

	public void ScoreReduce() {
		score--;
	}

	public void SetAll(ref GameObject starship)
	{
		SetStarshipColor (ref starship);
	}

	public void SetStarshipColor(ref GameObject starship) {

		try {
			GameObject gameObject = starship.transform.Find("colorparts").gameObject;
			Renderer renderer = gameObject.GetComponentsInChildren<Renderer> () [0];
			renderer.material.SetColor("_Color", Color.HSVToRGB(colorHue, colorSaturation, colorLuminance));
		}
		finally {
		}
	}
}