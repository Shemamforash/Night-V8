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
		private readonly List<GameObject> _skillCostBlips = new List<GameObject>();
		private          ParticleSystem   _unlockSparks;
		private          ParticleSystem   _unlockBurst;
		private          ParticleSystem   _readyParticles;
		private          Transform        _costTransform;
		private          Image            _cooldownFill;
		private          EnhancedText     _skillNameText;
		private          CanvasGroup      _unlockedCanvas;
		private          Skill            _skill;
		private          bool             _unlocked;
		private          bool             _initialised;

		private void Awake()
		{
			Initialise();
		}

		private void Initialise()
		{
			if (_initialised) return;
			_unlockedCanvas = gameObject.FindChildWithName<CanvasGroup>("Unlocked");
			_cooldownFill   = _unlockedCanvas.gameObject.FindChildWithName<Image>("Fill");
			_skillNameText  = _unlockedCanvas.gameObject.FindChildWithName<EnhancedText>("Text");
			_costTransform  = _unlockedCanvas.gameObject.FindChildWithName("Cost").transform;
			_unlockSparks   = _unlockedCanvas.gameObject.FindChildWithName<ParticleSystem>("Unlock Sparks");
			_unlockBurst    = _unlockedCanvas.gameObject.FindChildWithName<ParticleSystem>("Unlock Burst");
			_readyParticles = _unlockedCanvas.gameObject.FindChildWithName<ParticleSystem>("Ready");
			UpdateCooldownFill(1);
			_initialised = true;
		}

		public void UpdateCooldownFill(float normalisedValue)
		{
			float alpha = normalisedValue == 1 ? 1 : 0.4f;
			alpha = _skill == null ? 0f : alpha;
			bool playParticles = _skill != null && alpha == 1;
			if (playParticles       && !_readyParticles.isPlaying) _readyParticles.Play();
			else if (!playParticles && _readyParticles.isPlaying) _readyParticles.Stop();
			_unlockedCanvas.alpha    = alpha;
			_cooldownFill.fillAmount = normalisedValue;
		}

		private void UpdateSkillUI()
		{
			_unlockedCanvas.alpha = _skill == null ? 0f : 1f;
		}

		public Skill Skill => _skill;

		public void SetSkill(Skill skill)
		{
			Awake();
			_skill = skill;
			UpdateSkillUI();
			if (_skill == null) return;
			string skillName = _skill.Name;
			_skillNameText.SetText(skillName);
			SetCost(_skill.Cooldown);
			UpdateCooldownFill(_unlockedCanvas.alpha);
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

		public void Unlock()
		{
			_unlockBurst.Emit(50);
			_unlockSparks.Emit(20);
		}
	}
}