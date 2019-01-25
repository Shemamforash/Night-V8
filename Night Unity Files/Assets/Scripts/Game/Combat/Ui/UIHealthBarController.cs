using DG.Tweening;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Combat.Ui
{
    public class UIHealthBarController : MonoBehaviour
    {
        private SteppedProgressBar _healthBar;
        private Image _sicknessImage;
        private bool _cached;
        private RectTransform _rect;

        public void Awake()
        {
            _healthBar = gameObject.FindChildWithName<SteppedProgressBar>("Fill");
            _sicknessImage = gameObject.FindChildWithName<Image>("Sickness");
            _rect = GetComponent<RectTransform>();
        }

        public void SetValue(Number health, bool doFade)
        {
            float normalisedHealth = health.Max / 2000;
            _rect.anchorMax = new Vector2(normalisedHealth, _rect.anchorMax.y);
            _healthBar.SetValue(health.Normalised(), doFade);
        }

        public void SetSicknessLevel(float normalisedValue)
        {
            _sicknessImage.DOFillAmount(normalisedValue, 0.1f);
        }
    }
}