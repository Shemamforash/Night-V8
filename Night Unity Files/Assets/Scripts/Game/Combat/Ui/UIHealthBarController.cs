using Extensions;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Combat.Ui
{
	public class UIHealthBarController : MonoBehaviour
	{
		private bool               _cached;
		private SteppedProgressBar _healthBar;
		private RectTransform      _rect;

		public void Awake()
		{
			_healthBar = gameObject.FindChildWithName<SteppedProgressBar>("Fill");
			_rect      = GetComponent<RectTransform>();
		}

		public void SetValue(Number health, bool doFade)
		{
			float normalisedHealth = health.Max / 2000;
			_rect.anchorMax = new Vector2(normalisedHealth, _rect.anchorMax.y);
			_healthBar.SetValue(health.Normalised, doFade);
		}
	}
}