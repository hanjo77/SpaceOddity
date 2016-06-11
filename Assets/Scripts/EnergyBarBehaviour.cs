using UnityEngine;
using System.Collections;

public class EnergyBarBehaviour : MonoBehaviour {

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
	private float _leftPadding;

	// Use this for initialization
	void Start () {
		Vector2 objSize = GetComponent<RectTransform> ().sizeDelta;
		_innerWidth = (int)objSize.x;
		_innerHeight = (int)objSize.y;
		_leftPadding = (Screen.width-_innerWidth)/2;
		_rectBorder = new Rect (_leftPadding, padding, _innerWidth, _innerHeight);
		_rectBg = new Rect (_leftPadding, padding, _innerWidth, _innerHeight);
		_imgBg = GetRectangle (Color.white, _innerWidth, _innerHeight);
	}
	
	// Update is called once per frame
	void Update () {
	}

    void OnGUI()
    {
		GUI.DrawTexture(_rectBg, _imgBg, ScaleMode.StretchToFill);
		GUI.DrawTexture(_rectBar, _imgBar, ScaleMode.StretchToFill);
		GUI.DrawTexture(_rectBorder, barBorder, ScaleMode.StretchToFill);
    }

	public void UpdateBar(float barValue) {
		if (barValue >= 0.001f) {
			int tmpWidth = (int)(barValue * (float)_innerWidth);
			_colorHue = minColorHue+((maxColorHue-minColorHue)*barValue);
			_rectBar = new Rect (_leftPadding, padding, tmpWidth, _innerHeight);
			_imgBar = GetRectangle (Color.HSVToRGB (_colorHue, colorSaturation, colorValue), tmpWidth, _innerHeight);
		}
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
