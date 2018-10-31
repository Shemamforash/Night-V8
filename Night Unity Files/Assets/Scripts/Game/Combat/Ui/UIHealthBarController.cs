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
        private ParticleSystem _burnEffect;
        private SteppedProgressBar _healthBar;
        private Image _sicknessImage;
        private bool _cached;

        public void Awake()
        {
            _healthBar = gameObject.GetComponent<SteppedProgressBar>();
            _burnEffect = gameObject.FindChildWithName<ParticleSystem>("Burning");
            _sicknessImage = gameObject.FindChildWithName<Image>("Sickness");
        }

        public void SetValue(Number health)
        {
            _healthBar.SetValue(health.Normalised());
        }

        public void StartBurning()
        {
            if (_burnEffect.isPlaying) return;
            _burnEffect.Play();
        }

        public void SetSicknessLevel(float normalisedValue)
        {
            _sicknessImage.DOFillAmount(normalisedValue, 0.1f);
        }

        public void StopBurning()
        {
            _burnEffect.Stop();
        }
    }
}