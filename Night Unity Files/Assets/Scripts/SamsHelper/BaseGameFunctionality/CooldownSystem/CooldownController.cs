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

        private void Awake()
        {
            _cooldownFill = gameObject.FindChildWithName<Image>("Fill");
            _cooldownText = gameObject.FindChildWithName<EnhancedText>("Text");
            _canvasGroup = GetComponent<CanvasGroup>();
            _readyParticles = gameObject.FindChildWithName<ParticleSystem>("Ready");
            _cooldownFill.fillAmount = 1;
            UpdateCooldownFill(1, false);
        }

        public void Text(string text)
        {
            _cooldownText.SetText(text);
        }

        public void UpdateCooldownFill(float normalisedValue, bool canAfford)
        {
            bool shouldPlay = normalisedValue == 1 && canAfford;
            bool shouldStop = !shouldPlay && _readyParticles.isPlaying;
            shouldPlay = shouldPlay && !_readyParticles.isPlaying;
            if (shouldPlay) _readyParticles.Play();
            else if (shouldStop) _readyParticles.Stop();

            Color targetColor = normalisedValue == 1 ? Color.white : _cooldownNotReadyColor;
            _cooldownText.SetColor(targetColor);
            _cooldownFill.fillAmount = normalisedValue;
        }

        public void Reset()
        {
            UpdateCooldownFill(1, false);
        }

        public void SetVisible(bool visible)
        {
            _canvasGroup.alpha = visible ? 1 : 0;
        }
    }
}