using DG.Tweening;
using Extensions;
using Game.Gear.Armour;

using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
	public class UIArmourController : MonoBehaviour
	{
		private RectTransform _armourRect;
		private Tweener       _fadeTween;
		private Image         _fillBar;
		private Image         _working;

		public void Awake()
		{
			_armourRect = gameObject.FindChildWithName<RectTransform>("Bar");
			_fillBar    = gameObject.FindChildWithName<Image>("Fill");
			_working    = gameObject.FindChildWithName<Image>("Working");
		}

		public void UpdateArmour(ArmourController controller)
		{
			float leftOffset = 1 - controller.TotalProtection;
			_armourRect.anchorMin = new Vector2(leftOffset, 0f);
			_fillBar.fillAmount   = controller.FillLevel;
			_fillBar.SetAlpha(controller.Recharging ? 0.5f : 1f);
			_working.SetAlpha(controller.Recharging ? 0f : 1f);
			_working.transform.parent.gameObject.SetActive(controller.CurrentLevel != 0);
			if (!controller.DidJustTakeDamage()) return;
			_fillBar.color = Color.red;
			_fadeTween?.Complete();
			_fadeTween = _fillBar.DOColor(Color.white, 0.5f);
		}
	}
}