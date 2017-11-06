using System.Collections.Generic;
using System.Linq;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
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
        private readonly ScrollingMenuList _enemyList;
        private float _hitInfoTimerCurrent;
        private const float HitInfoTimerMax = 1f;

        private readonly TextMeshProUGUI _characterName,
            _characterHealthText,
            _ammoText,
            _weaponNameText,
            _reloadTimeRemaining,
            _hitInfo;

        public readonly TextMeshProUGUI ConditionsText;

        private readonly Slider _characterHealthSlider;
        private readonly List<GameObject> _magazineAmmo = new List<GameObject>();
        private Character _character;
        private float _criticalTarget;
        public readonly CooldownController DashCooldownController, 
            ClassSkillOneCooldownController, 
            ClassSkillTwoCooldownController, 
            WeaponSkillOneCooldownController, 
            WeaponSkillTwoCooldownController;

        public CombatUi(GameObject combatMenu)
        {
            GameObject playerContainer = combatMenu.transform.Find("Player").gameObject;
            _enemyList = Helper.FindChildWithName<ScrollingMenuList>(combatMenu, "Enemies");
            _enemyList.DontFadeItems();
            _magazineContent = Helper.FindChildWithName<Transform>(playerContainer, "Magazine").gameObject;
            _ammoPrefab = Resources.Load("Prefabs/Combat/Ammo Prefab") as GameObject;

            _characterName = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Name");
            _characterHealthText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Strength Remaining");
            _ammoText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Ammo Stock");
            _weaponNameText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Weapon");
            _reloadTimeRemaining = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Time Remaining");
            _hitInfo = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Hit Info");
            ConditionsText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Conditions");
            _hitInfo.color = new Color(1, 1, 1, 0);

            GameObject cooldownContainer = Helper.FindChildWithName(playerContainer, "Cooldowns");
            DashCooldownController = Helper.FindChildWithName<CooldownController>(cooldownContainer, "Dash");

            ClassSkillOneCooldownController = Helper.FindChildWithName<CooldownController>(cooldownContainer, "Skill 1");
            ClassSkillTwoCooldownController = Helper.FindChildWithName<CooldownController>(cooldownContainer, "Skill 2");
            WeaponSkillOneCooldownController = Helper.FindChildWithName<CooldownController>(cooldownContainer, "Skill 3");
            WeaponSkillTwoCooldownController = Helper.FindChildWithName<CooldownController>(cooldownContainer, "Skill 4");

            
            _characterHealthSlider = Helper.FindChildWithName<Slider>(playerContainer, "Health Bar");
        }

        public void ShowHitMessage(string message)
        {
            _hitInfo.text = message;
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
            _hitInfo.color = new Color(1, 1, 1, opacity);
        }

        public void UpdateCharacterHealth(MyValue health)
        {
            _characterHealthSlider.value = health.GetCurrentValue() / health.Max;
            _characterHealthText.text = (int) health.GetCurrentValue() + "/" + (int) health.Max;
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
            _ammoText.text = CombatManager.Scenario().Player().Inventory().GetResourceQuantity(InventoryResourceType.Ammo).ToString();
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
            _character = scenario.Player();
            ResetMagazine(_character.Weapon().Capacity);
            UpdateMagazine(_character.Weapon().GetRemainingAmmo());
            _characterName.text = _character.Name;
            _weaponNameText.text = _character.Weapon().Name + " (" + _character.Weapon().GetSummary() + ")";
            _enemyList.SetItems(new List<MyGameObject>(scenario.Enemies()));
            _enemyList.GetItems().ForEach(e => e.OnEnter(() => SetTarget((Enemy) e.GetLinkedObject()))); //CombatManager.SetCurrentTarget(e.GetLinkedObject())));
            _enemyList.GetItems()[0].GetNavigationButton().GetComponent<Button>().Select();
        }

        public void SetSkillCooldownNames()
        {
            Player p = CombatManager.Scenario().Player();
            if (p.ClassSkillOne != null) ClassSkillOneCooldownController.Text(p.ClassSkillOne.Name);
            if (p.ClassSkillTwo != null) ClassSkillTwoCooldownController.Text(p.ClassSkillTwo.Name);
            if (p.WeaponSkillOne != null) WeaponSkillOneCooldownController.Text(p.WeaponSkillOne.Name);
            if (p.WeaponSkillTwo != null) WeaponSkillTwoCooldownController.Text(p.WeaponSkillTwo.Name);
        }

        private void SetTarget(Enemy e)
        {
            CombatManager.SetCurrentTarget(e);
            e.EnemyView().GetNavigationButton().GetComponent<Button>().Select();
        }

        public void Remove(Enemy enemy)
        {
            EnemyView v = (EnemyView) _enemyList.GetItems().FirstOrDefault(i => i.GetLinkedObject() == enemy);
            v.Destroy();
            for (int i = 0; i < _enemyList.GetItems().Count; ++i)
            {
                EnemyView enemyView = (EnemyView) _enemyList.GetItems()[i];
                if (enemyView.GetLinkedObject() != enemy) continue;
                int newTarget = i + 1;
                if (newTarget == _enemyList.GetItems().Count)
                {
                    newTarget = i - 1;
                }
                if (newTarget != -1)
                {
                    SetTarget((Enemy) _enemyList.GetItems()[newTarget].GetLinkedObject());
                }
                _enemyList.Remove(enemyView);
                enemyView.Destroy();
                break;
            }
        }
    }
}