using UnityEngine;
using UnityEngine.UI;

public class ColourPulse : MonoBehaviour
{
    public Color From = Color.white;
    public Color To = Color.black;
    public float Duration = 1f;
    private float _currentTime;
    private Color _currentFrom, _currentTo;
    private SpriteRenderer _sprite;
    private Image _image;
    [SerializeField] private float _alphaMultiplier = 1f;

    public void Awake()
    {
        _currentFrom = From;
        _currentTo = To;
        _image = GetComponent<Image>();
        _sprite = GetComponent<SpriteRenderer>();
    }

    public void SetAlphaMultiplier(float multiplier)
    {
        multiplier = Mathf.Clamp(multiplier, 0f, 1f);
        _alphaMultiplier = multiplier;
    }

    public void Update()
    {
        Color col = new Color(1,1,1,0f);
        if (_alphaMultiplier != 0)
        {
            _currentTime += Time.deltaTime;
            if (_currentTime >= Duration)
            {
                _currentTime = 0f;
                Color temp = _currentFrom;
                _currentFrom = _currentTo;
                _currentTo = temp;
            }

            col = Color.Lerp(_currentFrom, _currentTo, _currentTime / Duration);
            if (_alphaMultiplier != 1) col.a *= _alphaMultiplier;
        }
        if (_sprite != null) _sprite.color = col;
        if (_image != null) _image.color = col;
    }
}