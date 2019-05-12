using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SteppedProgressBar : MonoBehaviour
{
	private RectTransform _rect;
	private Image         _slider;
	private RectTransform _sliderRect;

	public void Awake()
	{
		_slider = GetComponent<Image>();
		_rect   = GetComponent<RectTransform>();
		ResetValue();
	}

	private void ResetValue()
	{
		_slider.fillAmount = 1;
	}

	public void SetValue(float newValue, bool useFade = true)
	{
		float oldValue = _slider.fillAmount;
		if (oldValue == newValue) return;
		_slider.fillAmount = newValue;
		if (_slider.fillOrigin == (int) Image.OriginHorizontal.Right)
		{
			float temp = oldValue;
			oldValue = 1 - newValue;
			newValue = 1 - temp;
		}

		if (!useFade) return;
		Fader         fader          = CreateNewFadeBlock();
		RectTransform faderTransform = fader.GetComponent<RectTransform>();
		faderTransform.anchorMin = new Vector2(newValue, 0);
		faderTransform.anchorMax = new Vector2(oldValue, 1);
		faderTransform.offsetMin = Vector2.zero;
		faderTransform.offsetMax = Vector2.zero;
	}

	private Fader CreateNewFadeBlock()
	{
		GameObject faderObject = new GameObject();
		faderObject.name = "Fader";
		faderObject.transform.SetParent(_rect, false);
		faderObject.AddComponent<Image>();
		Fader fader = faderObject.AddComponent<Fader>();
		faderObject.transform.SetSiblingIndex(1);
		return fader;
	}

	private class Fader : MonoBehaviour
	{
		private const float Duration = 0.5f;
		private       Image _faderImage;

		public void Awake()
		{
			_faderImage = GetComponent<Image>();
			Sequence seq = DOTween.Sequence();
			_faderImage.color = Color.red;
			seq.Append(_faderImage.DOColor(new Color(1, 0, 0, 0), Duration));
			seq.AppendCallback(() =>
			{
				Destroy(gameObject);
//                gameObject.SetActive(false);
//                _faderPool.Add(this);
			});
		}
	}
}