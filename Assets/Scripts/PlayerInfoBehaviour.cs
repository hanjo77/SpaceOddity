using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerInfoBehaviour : MonoBehaviour
{
	void Start() {
	}

	public void SetContent(StarshipController controller) {
		Text nameText = transform.Find ("NameText").GetComponent<Text>();
		Text scoreText = transform.Find ("ScoreText").GetComponent<Text>();
		Prefs prefs = controller.prefs;
		nameText.text = prefs.playerName;
		scoreText.color = Color.HSVToRGB (prefs.colorHue, prefs.colorSaturation, prefs.colorLuminance);
		scoreText.text = "0";
	}
}