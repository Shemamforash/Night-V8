using System.Collections.Generic;
using System.Linq;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
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
            _regularRoundsText,
            _weaponNameText,
            _reloadTimeRemaining,
            _hitInfo;

        private readonly Slider _characterHealthSlider, _aimSlider;
        private readonly List<GameObject> _magazineAmmo = new List<GameObject>();
        private readonly RectTransform CriticalBar;
        private Character _character;
        private float _criticalTarget;

        public CombatUi(GameObject combatMenu)
        {
            GameObject playerContainer = combatMenu.transform.Find("Player").gameObject;
            _enemyList = Helper.FindChildWithName<ScrollingMenuList>(combatMenu, "Enemies");
            _magazineContent = Helper.FindChildWithName<Transform>(playerContainer, "Magazine").Find("Content").gameObject;
            _ammoPrefab = Resources.Load("Prefabs/Combat/Ammo Prefab") as GameObject;

            _characterName = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Name");
            _characterHealthText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Strength Remaining");
            _regularRoundsText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Ammo Stock");
            _weaponNameText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Weapon");
            _reloadTimeRemaining = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Time Remaining");
            _hitInfo = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Hit Info");
            CriticalBar = Helper.FindChildWithName<RectTransform>(playerContainer, "Critical Bar");
            _hitInfo.color = new Color(1, 1, 1, 0);

            _characterHealthSlider = Helper.FindChildWithName<Slider>(playerContainer, "Health Bar");
            _aimSlider = Helper.FindChildWithName<Slider>(playerContainer, "Aim Bar");
        }

        private void UpdateAimSlider()
        {
            _aimSlider.value = _character.CombatStates.AimAmount.GetCurrentValue();
            if (_aimSlider.value / 100 > _criticalTarget)
            {
                Helper.FindChildWithName<Image>(_aimSlider.gameObject, "Fill").color = Color.red;
            }
            else
            {
                Helper.FindChildWithName<Image>(_aimSlider.gameObject, "Fill").color = Color.white;
            }
        }

        private void SetCriticalBar()
        {
            _criticalTarget = 1 - _character.Weapon().WeaponAttributes.CriticalChance.GetCalculatedValue() / 100;
            CriticalBar.anchorMin = new Vector2(_criticalTarget, 0);
            CriticalBar.anchorMax = new Vector2(_criticalTarget, 1);
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
            _regularRoundsText.text = _magazineAmmo.Count + "/" + CombatManager.Scenario().Player().Inventory().GetResourceQuantity(InventoryResourceType.Ammo);
        }

        public void SetMagazineText(string text)
        {
            _reloadTimeRemaining.text = text;
        }

        public void Update()
        {
            UpdateHitMessage();
            UpdateAimSlider();
        }

        public void Start(CombatScenario scenario)
        {
            _character = scenario.Player();
            SetCriticalBar();
            ResetMagazine(_character.Weapon().Capacity);
            UpdateMagazine(_character.Weapon().GetRemainingAmmo());
            _characterName.text = _character.Name;
            _weaponNameText.text = _character.Weapon().Name;
            _enemyList.SetUnselectedItemAction((enemyView, isSelected) =>
            {
                RectTransform rect = enemyView.GetGameObject().GetComponent<RectTransform>();
                if (isSelected)
                {
                    enemyView.GetGameObject().GetComponent<HorizontalLayoutGroup>().padding.left = 0;
                    rect.localScale = new Vector2(1, 1);
                    return;
                }
                enemyView.GetGameObject().GetComponent<HorizontalLayoutGroup>().padding.left = 300;
                rect.localScale = new Vector2(0.8f, 0.8f);
            });
            _enemyList.SetItems(new List<MyGameObject>(scenario.Enemies()));
            _enemyList.GetItems().ForEach(e => e.OnEnter(() => CombatManager.SetCurrentTarget(e.GetLinkedObject())));
            _enemyList.GetItems()[0].GetNavigationButton().GetComponent<Button>().Select();
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