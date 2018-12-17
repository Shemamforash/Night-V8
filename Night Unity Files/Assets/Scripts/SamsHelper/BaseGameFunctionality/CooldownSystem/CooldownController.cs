using System;
using Boo.Lang;
using Game.Combat.Player;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper.BaseGameFunctionality.CooldownSystem
{
    public class CooldownController : MonoBehaviour
    {
        private readonly Color _cooldownNotReadyColor = UiAppearanceController.FadedColour;
        private readonly List<GameObject> _skillCostBlips = new List<GameObject>();
        private Transform _costTransform;
        private Image _cooldownFill;
        private EnhancedText _cooldownText;
        private CanvasGroup _canvasGroup;
        private ParticleSystem _readyParticles;
        private Skill _skill;
        private bool _ready;
        private Func<bool> _isSkillUnlocked;
        private bool _unlocked;

        private void Awake()
        {
            _cooldownFill = gameObject.FindChildWithName<Image>("Fill");
            _cooldownText = gameObject.FindChildWithName<EnhancedText>("Text");
            _costTransform = gameObject.FindChildWithName("Cost").transform;
            _canvasGroup = GetComponent<CanvasGroup>();
            _readyParticles = gameObject.FindChildWithName<ParticleSystem>("Ready");
            _cooldownFill.fillAmount = 1;
            Reset();
        }

        public void UpdateCooldownFill(float normalisedValue)
        {
            _ready = normalisedValue == 1;
            Color targetColor = normalisedValue == 1 ? Color.white : _cooldownNotReadyColor;
            _cooldownText.SetColor(targetColor);
            _cooldownFill.fillAmount = normalisedValue;
        }

        public void Update()
        {
            UpdateUI();
            bool shouldPlay = _ready && _skill != null && _skill.CanAfford();
            bool shouldStop = !shouldPlay && _readyParticles.isPlaying;
            shouldPlay = shouldPlay && !_readyParticles.isPlaying;
            if (shouldPlay) _readyParticles.Play();
            else if (shouldStop) _readyParticles.Stop();
        }

        public Skill Skill() => _skill;

        public void SetSkill(Skill skill, Func<bool> isSkillUnlocked)
        {
            _isSkillUnlocked = isSkillUnlocked;
            _skill = skill;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_unlocked) return;
            _unlocked = _isSkillUnlocked();
            _canvasGroup.alpha = _unlocked ? 1 : 0;
            _cooldownText.SetText(_unlocked ? _skill.Name : "");
            SetCost(_unlocked ? _skill.AdrenalineCost() : 0);
        }

        public void Reset()
        {
            UpdateCooldownFill(1);
        }

        public void SetCost(int cost)
        {
            _skillCostBlips.ForEach(s => Destroy(s));
            for (int i = 0; i < cost; ++i)
            {
                GameObject newBlip = Helper.InstantiateUiObject("Prefabs/AttributeMarkerPrefab", _costTransform);
                _skillCostBlips.Add(newBlip);
            }
        }
    }
}