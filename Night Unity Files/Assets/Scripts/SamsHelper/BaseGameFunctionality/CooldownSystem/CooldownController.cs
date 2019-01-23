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
        private Image _cooldownFill, _progressImage;
        private EnhancedText _cooldownText, _progressText;
        private CanvasGroup _unlockedCanvas, _lockedCanvas;
        private ParticleSystem _readyParticles;
        private Skill _skill;
        private Func<bool> _isSkillUnlocked;
        private bool _unlocked, _ready;
        private Func<Tuple<string, float>> _getProgress;

        private void Awake()
        {
            _lockedCanvas = gameObject.FindChildWithName<CanvasGroup>("Locked");
            _progressImage = _lockedCanvas.gameObject.FindChildWithName<Image>("Progress");
            _progressText = _lockedCanvas.gameObject.FindChildWithName<EnhancedText>("Text");

            _unlockedCanvas = gameObject.FindChildWithName<CanvasGroup>("Unlocked");
            _cooldownFill = _unlockedCanvas.gameObject.FindChildWithName<Image>("Fill");
            _cooldownText = _unlockedCanvas.gameObject.FindChildWithName<EnhancedText>("Text");
            _costTransform = _unlockedCanvas.gameObject.FindChildWithName("Cost").transform;
            _readyParticles = _unlockedCanvas.gameObject.FindChildWithName<ParticleSystem>("Ready");
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
            bool isPlaying = _readyParticles.isPlaying;
            bool shouldPlay = _ready && _skill != null && _skill.CanAfford() && _unlocked;
            bool shouldStop = !shouldPlay && isPlaying;
            shouldPlay = shouldPlay && !isPlaying;
            if (shouldPlay) _readyParticles.Play();
            else if (shouldStop) _readyParticles.Stop();
        }

        public Skill Skill() => _skill;

        public void SetSkill(Skill skill, Func<bool> isSkillUnlocked, Func<Tuple<string, float>> getProgress)
        {
            _isSkillUnlocked = isSkillUnlocked;
            _skill = skill;
            _getProgress = getProgress;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_unlocked || _isSkillUnlocked == null) return;
            _unlocked = _isSkillUnlocked();
            _lockedCanvas.alpha = _unlocked ? 0 : 1;
            _unlockedCanvas.alpha = _unlocked ? 1 : 0;
            if (_unlocked)
            {
                _cooldownText.SetText(_skill.Name);
                SetCost(_unlocked ? _skill.AdrenalineCost() : 0);
            }
            else
            {
                Tuple<string, float> progress = _getProgress();
                _progressText.SetText(progress.Item1);
                _progressImage.fillAmount = progress.Item2;
            }
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