using SamsHelper;
using UnityEngine;
using UnityEngine.UI;

public class UIAimController : MonoBehaviour
{
	private RectTransform _rightTransform, _leftTransform;
	private Image _rightImage, _leftImage;
	private float _currentTime;
	private float _currentAlpha;
	private ParticleSystem _firingParticles;
	private CanvasGroup _aimCanvas;
	
	// Use this for initialization
	public void Awake ()
	{
		_rightTransform = Helper.FindChildWithName<RectTransform>(gameObject, "Right");
		_leftTransform = Helper.FindChildWithName<RectTransform>(gameObject, "Left");
		_rightImage = Helper.FindChildWithName<Image>(gameObject, "Right");
		_leftImage = Helper.FindChildWithName<Image>(gameObject, "Left");
		_firingParticles = Helper.FindChildWithName<ParticleSystem>(gameObject, "Gun Fire");
		_aimCanvas = GetComponent<CanvasGroup>();
		SetValue(0);
	}

	public void Hide()
	{
		_aimCanvas.alpha = 0;
	}

	public void Show()
	{
		_aimCanvas.alpha = 1;
	}
	
	public void SetValue (float value)
	{
		float width = GetComponent<RectTransform>().rect.width;
		width /= 2;
		float position = width * value;
		float alpha = value * value;
		_leftImage.color = new Color(1,1,1,alpha);
		_rightImage.color = new Color(1,1,1,alpha);
		_leftTransform.anchoredPosition = new Vector2(position, 0);
		_rightTransform.anchoredPosition = new Vector2(-position, 0);
	}

	public void Fire()
	{
		SetValue(0);
		_currentAlpha = 1f;
		_firingParticles.Emit(2);
	}
}
