using System;
using System.Collections.Generic;
using Game.Combat.Enemies;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.InventoryUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Character = Game.Characters.Character;

namespace Game.Combat
{
    public partial class CombatManager
    {
        public static MenuList EnemyList;
        public static MenuList GrenadeList;
        public static CanvasGroup CombatCanvas, PlayerCanvasGroup;

        private static TextMeshProUGUI _playerName;

        private static TextMeshProUGUI _playerHealthText;

        private static TextMeshProUGUI _coverText;

        private static Image _dashRing;
//            _hitInfo;

        private static UIHealthBarController _playerUiHealthController;
        private float _criticalTarget;
        public static SkillBar SkillBar;

        public static void UpdateDashTimer(float amount)
        {
            _dashRing.fillAmount = amount;
        }

        public void Awake()
        {
            GameObject playerContainer = Helper.FindChildWithName(gameObject, "Player");
            PlayerCanvasGroup = playerContainer.GetComponent<CanvasGroup>();
            EnemyList = Helper.FindChildWithName<MenuList>(gameObject, "Enemies");
            GrenadeList = Helper.FindChildWithName<MenuList>(gameObject, "Grenades");

            CombatCanvas = Helper.FindChildWithName<CanvasGroup>(gameObject, "Combat Canvas");

            _playerName = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Name");
            _playerHealthText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Health");
            _dashRing = Helper.FindChildWithName<Image>(playerContainer, "Ring");

            _coverText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Cover");
            _coverText.text = "";
//            _hitInfo = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Hit Info");
//            _hitInfo.color = new Color(1, 1, 1, 0);

            SkillBar = Helper.FindChildWithName<SkillBar>(playerContainer, "Skill Bar");

            _playerUiHealthController = Helper.FindChildWithName<UIHealthBarController>(playerContainer, "Health Bar");
        }

        public static void SetCoverText(string coverText)
        {
            _coverText.text = coverText;
        }

        public static void UpdatePlayerHealth()
        {
            int currentHealth = (int) _player.HealthController.GetCurrentHealth();
            int maxHealth = (int) _player.HealthController.GetMaxHealth();
            _playerUiHealthController.SetValue(_player.HealthController.GetNormalisedHealthValue());
            _playerHealthText.text = currentHealth + "/" + maxHealth;
        }

        public static void SetTarget(Enemy e)
        {
            _currentTarget?.EnemyView.MarkUnselected();
            if (e == null) return;
            _currentTarget = e;
            e.EnemyView.PrimaryButton.GetComponent<Button>().Select();
        }

        public static void Remove(Enemy enemy)
        {
            Enemy nearestEnemy = null;
            int distanceToEnemy = 100;
            int enemyPosition = EnemyList.Items.IndexOf(enemy.EnemyView);
            for (int i = 0; i < EnemyList.Items.Count; ++i)
            {
                Enemy e = EnemyList.Items[i].GetLinkedObject() as Enemy;
                if (e == null || !EnemyList.Items[i].Navigatable()) continue;
                int distance = Math.Abs(enemyPosition - i);
                if (distance >= distanceToEnemy) continue;
                nearestEnemy = e;
                distanceToEnemy = distance;
            }

            SetTarget(nearestEnemy);
        }
    }
}