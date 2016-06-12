using UnityEngine;

public class CrossHairBehaviour : MonoBehaviour {

	public StarshipController starShip;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

	public Vector3 GetDistance(Transform other) {
		return (other.position - starShip.transform.position);
	}

	public void SetStarship(StarshipController ship) {
		starShip = ship;
		Prefs prefs = ship.prefs;
		Renderer[] renderers = transform.GetComponentsInChildren<Renderer> ();
		Color c = Color.HSVToRGB (prefs.colorHue, prefs.colorSaturation, prefs.colorLuminance);
		foreach (Renderer renderer in renderers) {
			renderer.material.SetColor("_Color", c);
		}
	}
}
