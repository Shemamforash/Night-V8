using System;
using Game.Characters;
using Game.Combat;
using NUnit.Framework;
using SamsHelper;
using SamsHelper.ReactiveUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class UIHealthBarController : MonoBehaviour
    {
        private Slider _slider;
        private ParticleSystem _healthParticles;
        private ParticleSystem _bleedEffect, _burnEffect;
        private RectTransform _sliderRect;
        private float _edgeWidthRatio = 3f;
        private readonly Number _healthRemaining = new Number();
        private event Action<int> OnTakeDamage;
        private event Action<int> OnHeal;
        private TextMeshProUGUI _healthText;
        private CharacterCombat _character;

        public void Awake()
        {
            GameObject healthBar = Helper.FindChildWithName(gameObject, "Health Bar");
            _slider = healthBar.GetComponent<Slider>();
            _healthParticles = Helper.FindChildWithName<ParticleSystem>(healthBar, "Health Effect");
            _burnEffect = Helper.FindChildWithName<ParticleSystem>(healthBar, "Burning");
            _bleedEffect = Helper.FindChildWithName<ParticleSystem>(healthBar, "Bleeding");
            _healthText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Health Text");
            SetValue(1);
            if (_slider.direction != Slider.Direction.LeftToRight) return;
            ParticleSystem.ShapeModule shapeModule = _healthParticles.shape;
            shapeModule.rotation = new Vector3(0, 0, 270);
            _healthRemaining.OnMin(() => GetCharacter()?.Kill());
            _healthRemaining.AddOnValueChange(a => SetValue(GetNormalisedHealthValue()));
        }

        private CharacterCombat GetCharacter()
        {
            return _character;
        }

        public void SetInitialHealth(int initialHealth, CharacterCombat character)
        {
            _character = character;
            _healthRemaining.Max = initialHealth;
            _healthRemaining.SetCurrentValue(initialHealth);
        }

        public void TakeDamage(int amount)
        {
            Assert.IsTrue(amount >= 0);
            if (amount == 0) return;
            if (_healthRemaining.ReachedMin()) return;
            _healthRemaining.Decrement(amount);
            OnTakeDamage?.Invoke(amount);
        }

        public void Heal(int amount)
        {
            Assert.IsTrue(amount >= 0);
            _healthRemaining.Increment(amount);
            OnHeal?.Invoke(amount);
        }

        public void AddOnTakeDamage(Action<int> a) => OnTakeDamage += a;
        public void AddOnHeal(Action<int> a) => OnHeal += a;

        public float GetNormalisedHealthValue()
        {
            return _healthRemaining.Normalised();
        }

        public float GetCurrentHealth()
        {
            return _healthRemaining.CurrentValue();
        }

        public float GetMaxHealth()
        {
            return _healthRemaining.Max;
        }

        public void SetIsPlayerBar()
        {
            _edgeWidthRatio = 7.2f;
        }

        public void SetValue(float alpha)
        {
            float normalisedHealth = GetNormalisedHealthValue();
            _healthText.text = (int) GetCurrentHealth() + "/" + (int) GetMaxHealth();

            int amount = 50;
            if (alpha == 0 || normalisedHealth < 0)
            {
                amount = 0;
            }
            else
            {
                _slider.value = normalisedHealth;
            }

            ParticleSystem.MainModule main = _healthParticles.main;
            main.startColor = new Color(1, 1, 1, alpha);
            _healthParticles.Emit(amount);

            float edgeWidth = _edgeWidthRatio * normalisedHealth;

            ParticleSystem.ShapeModule burnShape = _burnEffect.shape;
            burnShape.radius = edgeWidth;

            ParticleSystem.EmissionModule burnEmission = _burnEffect.emission;
            burnEmission.rateOverTime = (int) (edgeWidth / _edgeWidthRatio * 50f);

            ParticleSystem.ShapeModule bleedShapeModule = _bleedEffect.shape;
            bleedShapeModule.radius = edgeWidth;

            ParticleSystem.EmissionModule bleedEmission = _bleedEffect.emission;
            bleedEmission.rateOverTime = (int) (edgeWidth / _edgeWidthRatio * 10f);
        }

        public void StartBleeding()
        {
            if (_bleedEffect.isPlaying) return;
            _bleedEffect.Play();
        }

        public void StopBleeding()
        {
            _bleedEffect.Stop();
        }

        public void StartBurning()
        {
            if (_burnEffect.isPlaying) return;
            _burnEffect.Play();
        }

        public void StopBurning()
        {
            _burnEffect.Stop();
        }
    }
}