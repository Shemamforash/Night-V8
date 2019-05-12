using DG.Tweening;
using Extensions;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Global
{
	public class ResourceTab : MonoBehaviour
	{
		private Image           _glow;
		private int             _lastValue;
		private TextMeshProUGUI _text;
		private Tweener         _textTween, _glowTween;

		public void Awake()
		{
			_text = GetComponent<TextMeshProUGUI>();
			_glow = gameObject.FindChildWithName<Image>("Glow");
			_glow.SetAlpha(0);
		}

		public void UpdateTab(string resourceName, int quantity)
		{
			if (quantity == 0)
			{
				gameObject.SetActive(false);
				return;
			}

			gameObject.SetActive(true);
			Vector3 position = transform.position;
			position.z         = 0;
			transform.position = position;

			if (quantity == _lastValue) return;
			_text.text = quantity + " " + resourceName;
			if (quantity < _lastValue)
			{
				_textTween?.Complete();
				_text.color = Color.red;
				_textTween  = _text.DOColor(Color.white, 1f);
			}
			else
			{
				_glowTween?.Complete();
				_glow.SetAlpha(0.5f);
				_glowTween = _glow.DOFade(0f, 1f);
			}

			_lastValue = quantity;
		}
	}
}