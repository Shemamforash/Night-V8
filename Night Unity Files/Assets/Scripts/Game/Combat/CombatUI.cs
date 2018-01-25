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

        public static UIHealthBarController PlayerHealthBar;
        private float _criticalTarget;
        public static SkillBar SkillBar;

        public void Awake()
        {
            GameObject playerContainer = Helper.FindChildWithName(gameObject, "Player");
            PlayerCanvasGroup = playerContainer.GetComponent<CanvasGroup>();
            EnemyList = Helper.FindChildWithName<MenuList>(gameObject, "Enemies");
            GrenadeList = Helper.FindChildWithName<MenuList>(gameObject, "Grenades");

            CombatCanvas = Helper.FindChildWithName<CanvasGroup>(gameObject, "Combat Canvas");

            _playerName = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Name");
            _playerHealthText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Health");

            _coverText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Cover");
            _coverText.text = "";

            SkillBar = Helper.FindChildWithName<SkillBar>(playerContainer, "Skill Bar");

            PlayerHealthBar = Helper.FindChildWithName<UIHealthBarController>(playerContainer, "Health Bar");
            PlayerHealthBar.SetIsPlayerBar();
        }

        public static void SetCoverText(string coverText)
        {
            _coverText.text = coverText;
        }

        public static void UpdatePlayerHealth()
        {
            int currentHealth = (int) _player.HealthController.GetCurrentHealth();
            int maxHealth = (int) _player.HealthController.GetMaxHealth();
            PlayerHealthBar.SetValue(_player.HealthController.GetNormalisedHealthValue(), PlayerCanvasGroup.alpha);
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