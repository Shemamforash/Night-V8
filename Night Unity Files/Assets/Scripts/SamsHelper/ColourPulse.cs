using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ColourPulse : MonoBehaviour
{
	[SerializeField] private float          _alphaMultiplier = 1f;
	private                  Color          _currentFrom, _currentTo;
	private                  float          _currentTime;
	private                  Image          _image;
	private                  bool           _pulse = true;
	private                  SpriteRenderer _sprite;
	public                   float          Duration = 1f;
	public                   Color          From     = Color.white;
	public                   Color          To       = Color.black;

	public void Awake()
	{
		_currentFrom = From;
		_currentTo   = To;
		_image       = GetComponent<Image>();
		_sprite      = GetComponent<SpriteRenderer>();
	}

	public void SetAlphaMultiplier(float multiplier)
	{
		multiplier       = Mathf.Clamp(multiplier, 0f, 1f);
		_alphaMultiplier = multiplier;
	}

	public void Update()
	{
		if (!_pulse) return;
		Color col = new Color(1, 1, 1, 0f);
		if (_alphaMultiplier != 0)
		{
			_currentTime += Time.unscaledDeltaTime;
			if (_currentTime >= Duration)
			{
				_currentTime = 0f;
				Color temp = _currentFrom;
				_currentFrom = _currentTo;
				_currentTo   = temp;
			}

			col = Color.Lerp(_currentFrom, _currentTo, _currentTime / Duration);
			if (_alphaMultiplier != 1) col.a *= _alphaMultiplier;
		}

		if (_sprite != null) _sprite.color = col;
		if (_image  != null) _image.color  = col;
	}

	public void PulseAndEnd()
	{
		_pulse = false;
		StartCoroutine(EndPulse());
	}

	private IEnumerator EndPulse()
	{
		float currentTime = Duration;
		while (currentTime > 0)
		{
			currentTime -= Time.unscaledDeltaTime;
			Color col                          = Color.Lerp(_currentTo, _currentFrom, _currentTime / Duration);
			if (_sprite != null) _sprite.color = col;
			if (_image  != null) _image.color  = col;
			yield return null;
		}

		if (_sprite != null) _sprite.color = _currentFrom;
		if (_image  != null) _image.color  = _currentFrom;
		Destroy(this);
	}

	public float GetAlphaMultiplier() => _alphaMultiplier;
}