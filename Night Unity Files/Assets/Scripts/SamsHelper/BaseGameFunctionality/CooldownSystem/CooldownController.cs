using System;
using System.Collections.Generic;
using Game.Combat.Player;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper.BaseGameFunctionality.CooldownSystem
{
    public class CooldownController : MonoBehaviour
    {
        private readonly Color _cooldownNotReadyColor = UiAppearanceController.FadedColour;
        private readonly List<GameObject> _skillCostBlips = new List<GameObject>();
        private Transform _costTransform;
        private Image _cooldownFill, _progressImage;
        private EnhancedText _skillNameText, _progressText;
        private CanvasGroup _unlockedCanvas, _lockedCanvas;
        private Skill _skill;
        private Func<bool> _isSkillUnlocked;
        private bool _unlocked;
        private Func<Tuple<string, float>> _getProgress;

        private void Awake()
        {
            _lockedCanvas = gameObject.FindChildWithName<CanvasGroup>("Locked");
            _progressImage = _lockedCanvas.gameObject.FindChildWithName<Image>("Progress");
            _progressText = _lockedCanvas.gameObject.FindChildWithName<EnhancedText>("Text");

            _unlockedCanvas = gameObject.FindChildWithName<CanvasGroup>("Unlocked");
            _cooldownFill = _unlockedCanvas.gameObject.FindChildWithName<Image>("Fill");
            _skillNameText = _unlockedCanvas.gameObject.FindChildWithName<EnhancedText>("Text");
            _costTransform = _unlockedCanvas.gameObject.FindChildWithName("Cost").transform;
            _cooldownFill.fillAmount = 1;
            Reset();
        }

        public void UpdateCooldownFill(float normalisedValue)
        {
            Color targetColor = normalisedValue == 1 ? Color.white : _cooldownNotReadyColor;
            _skillNameText.SetColor(targetColor);
            _cooldownFill.fillAmount = normalisedValue;
        }

        public void Update()
        {
            if (_isSkillUnlocked == null) return;
            if (!_unlocked) _unlocked = _isSkillUnlocked();
            if (_unlocked)
            {
                _unlockedCanvas.alpha = !_skill.CanAfford() ? 0.6f : 1f;
                _lockedCanvas.alpha = 0f;
            }
            else
            {
                _unlockedCanvas.alpha = 0;
                _lockedCanvas.alpha = 1;
                Tuple<string, float> progress = _getProgress();
                _progressText.SetText(progress.Item1);
                _progressImage.fillAmount = progress.Item2;
            }
        }

        public Skill Skill() => _isSkillUnlocked() ? _skill : null;

        public void SetSkill(Skill skill, Func<bool> isSkillUnlocked, Func<Tuple<string, float>> getProgress)
        {
            _isSkillUnlocked = isSkillUnlocked;
            _skill = skill;
            _getProgress = getProgress;
            _skillNameText.SetText(_skill.Name);
            SetCost(_skill.AdrenalineCost());
        }

        public void Reset()
        {
            UpdateCooldownFill(1);
        }

        private void SetCost(int cost)
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