using DG.Tweening;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.UI;

public class SteppedProgressBar : MonoBehaviour
{
	private Image _bar1;
	private Image _bar2;

	public void Awake()
	{
		_bar1 = gameObject.FindChildWithName<Image>("Bar 1");
		_bar2 = gameObject.FindChildWithName<Image>("Bar 2");
		ResetValue();
	}

	public void ResetValue(float value = 1f)
	{
		_bar1.fillAmount = value;
		_bar2.fillAmount = value;
		_bar2.color = new Color(0,0,0,0);
	}

	public void SetValue(float newValue)
	{
		_bar2.fillAmount = _bar1.fillAmount;
		_bar2.color = _bar1.color;
		_bar2.DOColor(new Color(0, 0, 0, 0), 0.5f);
		_bar1.fillAmount = newValue;
	}
}
