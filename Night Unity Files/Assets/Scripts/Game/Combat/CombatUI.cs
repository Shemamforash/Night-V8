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
        private static GameObject _ammoPrefab;
        private static GameObject _magazineContent;
        private static MenuList _enemyList;
        private float _hitInfoTimerCurrent;
        private const float HitInfoTimerMax = 1f;

        private static TextMeshProUGUI _playerName;

        private static TextMeshProUGUI _playerHealthText;

        private static TextMeshProUGUI _ammoText;

        private static TextMeshProUGUI _weaponNameText;

        private static TextMeshProUGUI _reloadTimeRemaining;

        private static TextMeshProUGUI _statusText;
//            _hitInfo;

        public static TextMeshProUGUI ConditionsText;

        private static UIHealthBarController _playerUiHealthController;
        private static List<GameObject> _magazineAmmo = new List<GameObject>();
        private float _criticalTarget;
        public static CooldownController DashCooldownController;
        public static SkillBar SkillBar;

        public void Awake()
        {
            GameObject playerContainer = gameObject.transform.Find("Player").gameObject;
            _enemyList = Helper.FindChildWithName<MenuList>(gameObject, "Enemies");
            _magazineContent = Helper.FindChildWithName<Transform>(playerContainer, "Magazine").gameObject;
            _ammoPrefab = Resources.Load("Prefabs/Combat/Ammo Prefab") as GameObject;

            _playerName = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Name");
            _playerHealthText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Strength Remaining");
            _ammoText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Ammo Stock");
            _weaponNameText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Weapon");
            _reloadTimeRemaining = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Time Remaining");
            _statusText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Status");
            _statusText.text = "";
//            _hitInfo = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Hit Info");
            ConditionsText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Conditions");
//            _hitInfo.color = new Color(1, 1, 1, 0);

            DashCooldownController = Helper.FindChildWithName<CooldownController>(playerContainer, "Dash");

            SkillBar = Helper.FindChildWithName<SkillBar>(playerContainer, "Skill Bar");

            _playerUiHealthController = Helper.FindChildWithName<UIHealthBarController>(playerContainer, "Health Bar");
        }

        public static void SetCoverText(string coverText)
        {
            _statusText.text = coverText;
        }

        public static void UpdatePlayerHealth()
        {
            int currentHealth = (int) _player.HealthController.GetCurrentHealth();
            int maxHealth = (int) _player.HealthController.GetMaxHealth();
            _playerUiHealthController.SetValue(_player.HealthController.GetNormalisedHealthValue());
            _playerHealthText.text = currentHealth + "/" + maxHealth;
        }

        private static void ResetMagazine(int capacity)
        {
            EnableReloadTime(false);
            foreach (GameObject round in _magazineAmmo)
            {
                GameObject.Destroy(round);
            }

            _magazineAmmo.Clear();
            for (int i = 0; i < capacity; ++i)
            {
                GameObject newRound = Helper.InstantiateUiObject(_ammoPrefab, _magazineContent.transform);
                _magazineAmmo.Add(newRound);
            }
        }

        public static void EmptyMagazine()
        {
            EnableReloadTime(true);
        }

        public static void UpdateReloadTime(float time)
        {
            string reloadTimeString = (Mathf.Round(time * 10f) / 10f).ToString("0.0") + "secs remaining";
            _reloadTimeRemaining.text = reloadTimeString;
        }

        private static void EnableReloadTime(bool enable)
        {
            _reloadTimeRemaining.gameObject.SetActive(enable);
            _magazineContent.SetActive(!enable);
        }

        public static void UpdateMagazine(int remaining)
        {
            EnableReloadTime(false);
            for (int i = 0; i < _magazineAmmo.Count; ++i)
            {
                GameObject round = _magazineAmmo[i].transform.Find("Round").gameObject;
                round.SetActive(i < remaining);
            }

            _ammoText.text = _player.Weapon().GetRemainingMagazines() + " mags";
        }

        public static void UpdateReloadTimeText(string text)
        {
            _reloadTimeRemaining.text = text;
        }

        private static void SetTarget(Enemy e)
        {
            SetCurrentTarget(e);
            e?.EnemyView().PrimaryButton.GetComponent<Button>().Select();
        }

        public static void Remove(Enemy enemy)
        {
            Enemy nearestEnemy = null;
            int distanceToEnemy = 100;
            int enemyPosition = _enemyList.Items.IndexOf(enemy.EnemyView());
            for (int i = 0; i < _enemyList.Items.Count; ++i)
            {
                Enemy e = _enemyList.Items[i].GetLinkedObject() as Enemy;
                if (e == null || !_enemyList.Items[i].Navigatable()) continue;
                int distance = Math.Abs(enemyPosition - i);
                if (distance >= distanceToEnemy) continue;
                nearestEnemy = e;
                distanceToEnemy = distance;
            }

            SetTarget(nearestEnemy);
        }
    }
}