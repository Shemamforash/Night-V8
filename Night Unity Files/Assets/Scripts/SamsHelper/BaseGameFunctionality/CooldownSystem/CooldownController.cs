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
        private Image _cooldownFill;
        private EnhancedText _cooldownText;
        private CanvasGroup _canvasGroup;
        private ParticleSystem _readyParticles;
        private Skill _skill;
        private bool _ready;

        private void Awake()
        {
            _cooldownFill = gameObject.FindChildWithName<Image>("Fill");
            _cooldownText = gameObject.FindChildWithName<EnhancedText>("Text");
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
            bool shouldPlay = _ready && _skill.CanAfford();
            bool shouldStop = !shouldPlay && _readyParticles.isPlaying;
            shouldPlay = shouldPlay && !_readyParticles.isPlaying;
            if (shouldPlay) _readyParticles.Play();
            else if (shouldStop) _readyParticles.Stop();
        }

        public void SetSkill(Skill skill)
        {
            _skill = skill;
            _canvasGroup.alpha = skill == null ? 0 : 1;
            _cooldownText.SetText(skill == null ? "" : skill.Name);
        }

        public void Reset()
        {
            UpdateCooldownFill(1);
        }
    }
}