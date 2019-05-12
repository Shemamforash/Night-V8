using DG.Tweening;
using Extensions;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
	public class GlowButtonBehaviour : MonoBehaviour
	{
		private Image _glow, _icon;

		private void Awake()
		{
			_glow = gameObject.FindChildWithName<Image>("Glow");
			_icon = gameObject.FindChildWithName<Image>("Not Equipped");
			_icon.SetAlpha(0.5f);
			_glow.SetAlpha(0f);
		}

		public void Deselect()
		{
			Fade(_icon, 0.5f);
			Fade(_glow, 0f);
		}

		private static void Fade(Image image, float to)
		{
			image.DOFade(to, 0.5f).SetUpdate(UpdateType.Normal, true);
		}

		public void Highlight()
		{
			Fade(_icon, 0.8f);
			Fade(_glow, 0.6f);
		}

		public void Select()
		{
			_icon.color = Color.white;
			_glow.color = Color.white;
			Deselect();
		}
	}
}