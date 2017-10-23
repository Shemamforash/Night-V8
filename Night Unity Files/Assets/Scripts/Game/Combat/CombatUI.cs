using System.Collections.Generic;
using Game.Combat.Enemies;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.InventoryUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Combat
{
    public class CombatUi
    {
        private readonly GameObject _ammoPrefab, _magazineContent;
        private readonly ScrollingMenuList _enemyList;

        public readonly TextMeshProUGUI CharacterName,
            CharacterHealthText,
            RegularRoundsText,
            WeaponNameText,
            ReloadTimeRemaining;

        private readonly Slider _characterHealthSlider, _aimSlider;
        private readonly List<GameObject> _magazineAmmo = new List<GameObject>();

        public CombatUi(GameObject combatMenu)
        {
            GameObject playerContainer = combatMenu.transform.Find("Player").gameObject;
            _enemyList = Helper.FindChildWithName<ScrollingMenuList>(combatMenu, "Enemies");
            _magazineContent = Helper.FindChildWithName<Transform>(playerContainer, "Magazine").Find("Content").gameObject;
            _ammoPrefab = Resources.Load("Prefabs/Combat/Ammo Prefab") as GameObject;

            CharacterName = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Name");
            CharacterHealthText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Strength Remaining");
            RegularRoundsText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Ammo Stock");
            WeaponNameText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Weapon");
            ReloadTimeRemaining = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Time Remaining");

            _characterHealthSlider = Helper.FindChildWithName<Slider>(playerContainer, "Health Bar");
            _aimSlider = Helper.FindChildWithName<Slider>(playerContainer, "Aim Bar");
        }

        public void ResetMagazine(int capacity)
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

        public void UpdateAimSlider(float value)
        {
            _aimSlider.value = value;
        }

        public void EmptyMagazine()
        {
            EnableReloadTime(true);
        }

        public void UpdateReloadTime(float time)
        {
            ReloadTimeRemaining.text = (Mathf.Round(time * 10f) / 10f).ToString("0.0") + "secs remaining";
        }

        private void EnableReloadTime(bool enable)
        {
            ReloadTimeRemaining.gameObject.SetActive(enable);
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
        }

        public void SetMagazineText(string text)
        {
            ReloadTimeRemaining.text = text;
        }

        public void SetEncounter(CombatScenario scenario)
        {
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
        }
    }
}