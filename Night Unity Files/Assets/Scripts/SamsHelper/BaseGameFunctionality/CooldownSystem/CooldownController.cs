using System;
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
		private          Transform        _costTransform;
		private          Image            _cooldownFill;
		private          EnhancedText     _skillNameText;
		private          CanvasGroup      _unlockedCanvas;
		private          Skill            _skill;
		private          bool             _unlocked;

		private void Awake()
		{
			_unlockedCanvas          = gameObject.FindChildWithName<CanvasGroup>("Unlocked");
			_cooldownFill            = _unlockedCanvas.gameObject.FindChildWithName<Image>("Fill");
			_skillNameText           = _unlockedCanvas.gameObject.FindChildWithName<EnhancedText>("Text");
			_costTransform           = _unlockedCanvas.gameObject.FindChildWithName("Cost").transform;
			UpdateCooldownFill(1);
		}

		public void UpdateCooldownFill(float normalisedValue)
		{
			Color targetColor = normalisedValue == 1 ? Color.white : _cooldownNotReadyColor;
			_skillNameText.SetColor(targetColor);
			_cooldownFill.fillAmount = normalisedValue;
		}

		private void UpdateSkillUI()
		{
			_unlockedCanvas.alpha = _skill == null ? 0f : 1f;
		}

		public Skill Skill => _skill;

		public void SetSkill(Skill skill)
		{
			_skill = skill;
			UpdateSkillUI();
			if (_skill == null) return;
			string skillName = _skill.Name;
			_skillNameText.SetText(skillName);
			SetCost(_skill.Cooldown);
		}

		private void SetCost(int cost)
		{
			_skillCostBlips.ForEach(Destroy);
			for (int i = 0; i < cost; ++i)
			{
				GameObject newBlip = Helper.InstantiateUiObject("Prefabs/AttributeMarkerPrefab", _costTransform);
				_skillCostBlips.Add(newBlip);
			}
		}
	}
}