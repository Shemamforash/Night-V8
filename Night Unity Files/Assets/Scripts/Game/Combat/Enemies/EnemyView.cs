using System.Runtime.InteropServices;
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
        public UIHealthBarController HealthBar;
        public UIAimController UiAimController;
        private float _currentFadeInTime = 2f;
        private const float MaxFadeInTime = 2f;
        private bool _inSight;
        private string _actionString;
        private Enemy _enemy;

        public EnemyView(MyGameObject linkedObject, Transform parent) : base(linkedObject, parent, "Prefabs/Inventory/EnemyItem")
        {
            GameObject.SetActive(true);
            SetAlpha(0f);
            _enemy = (Enemy) linkedObject;
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

        public void UpdateHealth()
        {
            if (!_inSight) return;
            HealthController health = _enemy.HealthController;
            HealthBar.SetValue(health.GetNormalisedHealthValue(), CurrentAlpha);
            HealthText.text = (int) health.GetCurrentHealth() + "/" + (int) health.GetMaxHealth();
        }

        public void SetActionText(string action)
        {
            _actionString = action;
            if (!_inSight) action = "";
            ActionText.text = action;
        }
        
        protected override void UpdateDistanceText()
        {
            if (_inSight)
            {
                base.UpdateDistanceText();
                return;
            }

            DistanceText.text = "behind";
        }

        public void Hide()
        {
            _inSight = false;
            UiAimController.Hide();
            UpdateHealth();
            UpdateDistanceText();
            SetActionText(_actionString);
        }

        public void Show()
        {
            _inSight = true;
            UiAimController.Show();
            SetActionText(_actionString);
            UpdateDistanceText();
            UpdateHealth();
        }

        public void SetArmour(int armourLevel, bool inCover)
        {
            if (!_inSight) return;
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

        public void MarkSelected()
        {
            PrimaryButton.GetComponent<Button>().Select();
            CharacterPositionManager.UpdatePlayerDirection();
        }
        
        public void MarkUnselected()
        {
            HealthBar.SetValue(-1, 0f);
        }
    }
}
