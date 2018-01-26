using System.Collections;
using Game.Characters.Player;
using Game.Combat.Enemies.EnemyTypes.Misc;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using TMPro;
using UnityEngine;

namespace Game.Combat.Enemies
{
    public class EnemyView : BasicEnemyView
    {
        public TextMeshProUGUI CoverText, ActionText, HealthText, ArmourText;
        private UIArmourController _uiArmourController;
        public UIHealthBarController HealthBar;
        public UIAimController UiAimController;
        private float _currentFadeInTime = 2f;
        private const float MaxFadeInTime = 2f;

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
            UiAimController = Helper.FindChildWithName<UIAimController>(GameObject, "Aim Timer");
            _uiArmourController = Helper.FindChildWithName<UIArmourController>(GameObject, "Armour Bar");
            HealthBar = Helper.FindChildWithName<UIHealthBarController>(GameObject, "Health Bar");
            ActionText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Action");
        }

        public void SetHealth(HealthController healthController)
        {
            HealthBar.SetValue(healthController.GetNormalisedHealthValue(), CurrentAlpha);
            HealthText.text = (int) healthController.GetCurrentHealth() + "/" + (int) healthController.GetMaxHealth();
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

        private IEnumerator FadeIn()
        {
            while (_currentFadeInTime > 0)
            {
                _currentFadeInTime -= Time.deltaTime;
                SetAlpha(CurrentAlpha);
                float fadeInAmount = 1f - _currentFadeInTime / MaxFadeInTime;
                float newAlpha = CurrentAlpha * fadeInAmount;
                GetGameObject().GetComponent<CanvasGroup>().alpha = newAlpha;
                yield return null;
            }
        }

        public void MarkUnselected()
        {
            HealthBar.SetValue(-1, 0f);
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
    }
}