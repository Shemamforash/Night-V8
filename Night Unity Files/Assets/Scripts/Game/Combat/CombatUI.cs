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
    public class CombatUi
    {
        private readonly GameObject _ammoPrefab, _magazineContent;
        private readonly MenuList _enemyList;
        private float _hitInfoTimerCurrent;
        private const float HitInfoTimerMax = 1f;

        private readonly TextMeshProUGUI _characterName,
            _characterHealthText,
            _ammoText,
            _weaponNameText,
            _reloadTimeRemaining;
//            _hitInfo;

        public readonly TextMeshProUGUI ConditionsText;

        private readonly UIHealthBarController _characterUiHealthController;
        private readonly List<GameObject> _magazineAmmo = new List<GameObject>();
        private Character _character;
        private float _criticalTarget;
        public readonly CooldownController DashCooldownController;
        public readonly SkillBar SkillBar;

        public CombatUi(GameObject combatMenu)
        {
            GameObject playerContainer = combatMenu.transform.Find("Player").gameObject;
            _enemyList = Helper.FindChildWithName<MenuList>(combatMenu, "Enemies");
            _magazineContent = Helper.FindChildWithName<Transform>(playerContainer, "Magazine").gameObject;
            _ammoPrefab = Resources.Load("Prefabs/Combat/Ammo Prefab") as GameObject;

            _characterName = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Name");
            _characterHealthText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Strength Remaining");
            _ammoText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Ammo Stock");
            _weaponNameText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Weapon");
            _reloadTimeRemaining = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Time Remaining");
//            _hitInfo = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Hit Info");
            ConditionsText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Conditions");
//            _hitInfo.color = new Color(1, 1, 1, 0);

            GameObject cooldownContainer = Helper.FindChildWithName(playerContainer, "Cooldowns");
            DashCooldownController = Helper.FindChildWithName<CooldownController>(cooldownContainer, "Dash");
            
            SkillBar = Helper.FindChildWithName<SkillBar>(playerContainer, "Skill Bar");

            _characterUiHealthController = Helper.FindChildWithName<UIHealthBarController>(playerContainer, "Health Bar");
        }

        public void ShowHitMessage(string message)
        {
//            _hitInfo.text = message;
            _hitInfoTimerCurrent = HitInfoTimerMax;
        }

        private void UpdateHitMessage()
        {
            if (!(_hitInfoTimerCurrent > 0)) return;
            _hitInfoTimerCurrent -= Time.deltaTime;
            float opacity = _hitInfoTimerCurrent / HitInfoTimerMax;
            if (opacity < 0)
            {
                opacity = 0;
            }
//            _hitInfo.color = new Color(1, 1, 1, opacity);
        }

        public void UpdateCharacterHealth(MyValue health)
        {
            _characterUiHealthController.SetValue(health.CurrentValue() / health.Max);
            _characterHealthText.text = (int) health.CurrentValue() + "/" + (int) health.Max;
        }

        private void ResetMagazine(int capacity)
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


        public void EmptyMagazine()
        {
            EnableReloadTime(true);
        }

        public void UpdateReloadTime(float time)
        {
            _reloadTimeRemaining.text = (Mathf.Round(time * 10f) / 10f).ToString("0.0") + "secs remaining";
        }

        private void EnableReloadTime(bool enable)
        {
            _reloadTimeRemaining.gameObject.SetActive(enable);
            _magazineContent.SetActive(!enable);
        }

        public void UpdateMagazine(int remaining)
        {
            EnableReloadTime(false);
            for (int i = 0; i < _magazineAmmo.Count; ++i)
            {
                GameObject round = _magazineAmmo[i].transform.Find("Round").gameObject;
                round.SetActive(i < remaining);
            }
            _ammoText.text = CombatManager.Player().Inventory().GetResourceQuantity(InventoryResourceType.Ammo).ToString();
        }

        public void SetMagazineText(string text)
        {
            _reloadTimeRemaining.text = text;
        }

        public void Update()
        {
            UpdateHitMessage();
        }

        public void Start(CombatScenario scenario)
        {
            _character = CombatManager.Player();
            ResetMagazine((int) _character.Weapon().WeaponAttributes.Capacity.CurrentValue());
            UpdateMagazine(_character.Weapon().GetRemainingAmmo());
            _characterName.text = _character.Name;
            _weaponNameText.text = _character.Weapon().Name + " (" + _character.Weapon().GetSummary() + ")";
            _enemyList.SetItems(new List<MyGameObject>(scenario.Enemies()));
            _enemyList.Items.ForEach(e => e.PrimaryButton.AddOnSelectEvent(() => SetTarget((Enemy) e.GetLinkedObject()))); //CombatManager.SetCurrentTarget(e.GetLinkedObject())));
            SetTarget((Enemy) _enemyList.Items[0].GetLinkedObject());
        }

        private void SetTarget(Enemy e)
        {
            CombatManager.SetCurrentTarget(e);
            e?.EnemyView().GetNavigationButton().GetComponent<Button>().Select();
        }

        public void Remove(Enemy enemy)
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