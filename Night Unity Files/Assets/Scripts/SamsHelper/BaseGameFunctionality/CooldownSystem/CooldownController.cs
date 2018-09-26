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
            UpdateCooldownFill(1);
        }

        public void Text(string text)
        {
            _cooldownText.SetText(text);
        }

        public void UpdateCooldownFill(float normalisedValue)
        {
            Color targetColor = _cooldownNotReadyColor;
            if (normalisedValue == 1)
            {
                targetColor = Color.white;
                if (!_readyParticles.isPlaying) _readyParticles.Play();
            }
            else if (_readyParticles.isPlaying) _readyParticles.Stop();

            _cooldownText.SetColor(targetColor);
            _cooldownFill.fillAmount = normalisedValue;
        }

        public void Reset()
        {
            UpdateCooldownFill(1);
        }

        public void SetVisible(bool visible)
        {
            _canvasGroup.alpha = visible ? 1 : 0;
        }
    }
}