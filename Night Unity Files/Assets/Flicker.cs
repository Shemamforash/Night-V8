using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Flicker : MonoBehaviour
{
	private Image _fireImage;
	private float _x, _y;
	public float FlickerRate = 0.1f;
	public float ClampMin, ClampMax = 1;
	
	// Use this for initialization
	public void Start ()
	{
		_fireImage = GetComponent<Image>();
	}
	
	// Update is called once per frame
	public void Update ()
	{
		_x += Random.Range(0f, FlickerRate);
		_y += Random.Range(0f, FlickerRate);
		if (_x > 1) _x -= 1;
		if (_y > 1) _y -= 1;
		float newOpacity = Mathf.PerlinNoise(_x, _y);
		if (ClampMin >= ClampMax)
		{
			throw new Exception("Clamp min should not be greater than or equal to clamp max.");
		}
		float clampDiff = ClampMax - ClampMin;
		newOpacity = newOpacity * clampDiff + ClampMin;
		Color oldColor = _fireImage.color;
		oldColor.a = newOpacity;
		_fireImage.color = oldColor;
	}
}
