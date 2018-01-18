using Game.Characters.Player;
using Game.Combat.Enemies.EnemyTypes.Misc;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Combat.Enemies
{
    public class EnemyView : BasicEnemyView
    {
        public TextMeshProUGUI CoverText, ActionText, HealthText, ArmourText;
        private UIArmourController _uiArmourController;
        private UIHealthBarController _lowerUiHealthBarController;
        private Image _sicknessLevel;
        private ParticleSystem _bleedEffect, _burnEffect;
        public UIAimController UiAimController;
        private float _fadingIn = 2f;
        private float _fadeInTime = 2f;

        public EnemyView(MyGameObject linkedObject, Transform parent) : base(linkedObject, parent, "Prefabs/Inventory/EnemyItem")
        {
            GameObject.SetActive(true);
            SetAlpha(0f);
        }

        protected override void CacheUiElements()
        {
            base.CacheUiElements();
            CoverText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Cover");
            CoverText.text = "No Cover";
            HealthText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Health Text");
            ArmourText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Armour Text");
            _sicknessLevel = Helper.FindChildWithName<Image>(GameObject, "Sickness");
            _burnEffect = Helper.FindChildWithName<ParticleSystem>(GameObject, "Burning");
            _bleedEffect = Helper.FindChildWithName<ParticleSystem>(GameObject, "Bleeding");
            UiAimController = Helper.FindChildWithName<UIAimController>(GameObject, "Aim Timer");
            _uiArmourController = Helper.FindChildWithName<UIArmourController>(GameObject, "Armour Bar");
            _lowerUiHealthBarController = Helper.FindChildWithName<UIHealthBarController>(GameObject, "Health Bar");
            ActionText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Action");
        }

        public void SetHealth(HealthController healthController)
        {
            _lowerUiHealthBarController.SetValue(healthController.GetNormalisedHealthValue());
            HealthText.text = (int) healthController.GetCurrentHealth() + "/" + (int) healthController.GetMaxHealth();
        }

        public void StartBleeding()
        {
            _bleedEffect.Play();
        }

        public void StopBleeding()
        {
            _bleedEffect.Stop();
        }

        public void StartBurning()
        {
            _burnEffect.Play();
        }

        public void StopBurning()
        {
            _burnEffect.Stop();
        }

        public void SetArmour(int armourLevel, bool inCover)
        {
            _uiArmourController.SetArmourValue(armourLevel);
            float armourProtection = 1 - armourLevel / 10f;
            string coverString = "No Cover";
            if (inCover)
            {
                armourProtection /= 2f;
                coverString = "In Cover";
            }

            CoverText.text = coverString;
            ArmourText.text = armourProtection + "x damage";
        }

        public void SetAlpha(float alpha)
        {
            if (_fadingIn > 0)
            {
                float fadeInAmount = 1f - _fadingIn / _fadeInTime;
                alpha *= fadeInAmount;
                _fadingIn -= Time.deltaTime;
            }

            GetGameObject().GetComponent<CanvasGroup>().alpha = alpha;
        }

        public void MarkUnselected()
        {
            _lowerUiHealthBarController.SetParticleEmissionOverDistance();
        }

        public void MarkDead()
        {
//            CoverText.text = "";
            DistanceText.text = "";
//            StrengthText.text = "";
//            ArmourText.text = "";
            _nameText.text = "Dead " + _nameText.text;
            ActionText.text = "";
            SetNavigatable(false);
        }

        public void UpdateSickness(float value)
        {
            _sicknessLevel.fillAmount = value;
        }
    }
}