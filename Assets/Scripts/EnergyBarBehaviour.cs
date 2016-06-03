using UnityEngine;
using System.Collections;

public class EnergyBarBehaviour : MonoBehaviour {

	public StarshipController starShip;
    public Texture2D barBorder;
    public int padding = 20;
    public float maxColorHue = .33f;
    public float minColorHue = 0;
    public float colorSaturation = 1;
    public float colorValue = 1;
    public float barValue = 1;
	private Texture2D _imgBg;
	private Texture2D _imgBorder;
	private Texture2D _imgBar;
	private Rect _rectBg;
	private Rect _rectBorder;
	private Rect _rectBar;
	private int _innerWidth, _innerHeight;
	private float _colorHue = -1f;

	// Use this for initialization
	void Start () {
		_innerWidth = barBorder.width - (int)(1.5f * padding);
		_innerHeight = barBorder.height - (3 * padding);
		_rectBorder = new Rect (0, 0, barBorder.width, barBorder.height);
		_rectBg = new Rect (padding * .75f, padding * 1.5f, _innerWidth, _innerHeight);
		_imgBg = GetRectangle (Color.white, _innerWidth, _innerHeight);
	}
	
	// Update is called once per frame
	void Update () {
		if (starShip) {
			if (_colorHue < 0 || starShip.energy != barValue) {
				UpdateBar ();
			}
		}
	}

    void OnGUI()
    {
		GUI.DrawTexture(_rectBg, _imgBg, ScaleMode.ScaleToFit);
		GUI.DrawTexture(_rectBar, _imgBar, ScaleMode.ScaleToFit);
		GUI.DrawTexture(_rectBorder, barBorder, ScaleMode.ScaleToFit);
    }

	void UpdateBar() {
		barValue = starShip.energy;
		_innerWidth = (int)(barValue * (float)_innerWidth);
		_colorHue = minColorHue+((maxColorHue-minColorHue)*barValue);
		_rectBar = new Rect (padding * .75f, padding * 1.5f, _innerWidth, _innerHeight);
		_imgBar = GetRectangle (Color.HSVToRGB (_colorHue, colorSaturation, colorValue), _innerWidth, _innerHeight);
	}

    Texture2D GetRectangle(Color color, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        int i, j;
        for (i = 0; i < width; i++)
        {
            for (j = 0; j < height; j++)
            {
                texture.SetPixel(i, j, color);
            }
        }
        texture.Apply();
        return texture;
    }
}
