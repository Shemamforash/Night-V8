using System.Collections.Generic;
using Extensions;
using Game.Combat.Player;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper.BaseGameFunctionality.CooldownSystem
{
	public class CooldownController : MonoBehaviour
	{
		private readonly Color            _cooldownNotReadyColor = UiAppearanceController.FadedColour;
		private readonly List<GameObject> _skillCostBlips        = new List<GameObject>();
		private          Image            _cooldownFill;
		private          Transform        _costTransform;
		private          Skill            _skill;
		private          EnhancedText     _skillNameText;
		private          bool             _unlocked;
		private          CanvasGroup      _unlockedCanvas;

		public Skill Skill
		{
			get => _skill;
			set
			{
				_skill = value;
				UpdateColour();
				SetCooldown(_skill.Cooldown);
				string skillNameText = _skill.Null() ? "" : _skill.Name;
				_skillNameText.SetText(skillNameText);
			}
		}

		private void Awake()
		{
			_unlockedCanvas          = GetComponent<CanvasGroup>();
			_cooldownFill            = _unlockedCanvas.gameObject.FindChildWithName<Image>("Fill");
			_skillNameText           = _unlockedCanvas.gameObject.FindChildWithName<EnhancedText>("Text");
			_costTransform           = _unlockedCanvas.gameObject.FindChildWithName("Cost").transform;
			_cooldownFill.fillAmount = 1;
			UpdateCooldownFill(1);
		}

		public void UpdateCooldownFill(float normalisedValue)
		{
			Color targetColor = normalisedValue == 1 ? Color.white : _cooldownNotReadyColor;
			_skillNameText.SetColor(targetColor);
			_cooldownFill.fillAmount = normalisedValue;
		}


		private void UpdateColour()
		{
			float unlockedAlpha = _skill.Null() ? 0f : 1f;
			_unlockedCanvas.alpha = unlockedAlpha;
		}

		private void SetCooldown(int duration)
		{
			_skillCostBlips.ForEach(Destroy);
			for (int i = 0; i < duration; ++i)
			{
				GameObject newBlip = Helper.InstantiateUiObject("Prefabs/AttributeMarkerPrefab", _costTransform);
				_skillCostBlips.Add(newBlip);
			}
		}
	}
}