using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Combat
{
    public class CombatUI
    {
        public GameObject AmmoPrefab, MagazineContent;
        private GameObject _combatMenu;

        public Text CharacterName,
            CharacterHealthText,
            RegularRoundsText,
            InfernalRoundsText,
            WeaponNameText,
            ExposureText;

        private Button _flankButton, _approachButton, _retreatButton, _coverButton;
        private Slider _characterHealthSlider, _aimSlider;
        private List<GameObject> _magazineAmmo = new List<GameObject>();

        public CombatUI(GameObject combatMenu)
        {
            _combatMenu = combatMenu;
            MagazineContent = Helper.FindChildWithName<Transform>(_combatMenu, "Magazine").Find("Content").gameObject;
            AmmoPrefab = Resources.Load("Prefabs/AmmoPrefab") as GameObject;

            CharacterName = Helper.FindChildWithName<Text>(_combatMenu, "Name");
            CharacterHealthText = Helper.FindChildWithName<Text>(_combatMenu, "Strength Remaining");
            RegularRoundsText = Helper.FindChildWithName<Text>(_combatMenu, "Regular");
            InfernalRoundsText = Helper.FindChildWithName<Text>(_combatMenu, "Infernal");
            WeaponNameText = Helper.FindChildWithName<Text>(_combatMenu, "Weapon");
            ExposureText = Helper.FindChildWithName<Text>(_combatMenu, "Exposure");

            _flankButton = Helper.FindChildWithName<Button>(_combatMenu, "Flank");
            _approachButton = Helper.FindChildWithName<Button>(_combatMenu, "Approach");
            _retreatButton = Helper.FindChildWithName<Button>(_combatMenu, "Retreat");
            _coverButton = Helper.FindChildWithName<Button>(_combatMenu, "Take Cover");

            _characterHealthSlider = Helper.FindChildWithName<Slider>(_combatMenu, "Health Bar");
            _aimSlider = Helper.FindChildWithName<Slider>(_combatMenu, "Aim Bar");
        }

        public void ResetMagazine(int capacity)
        {
            _magazineAmmo.Clear();
            foreach (GameObject round in _magazineAmmo)
            {
                GameObject.Destroy(round);
                _magazineAmmo.Add(round);
            }
            for (int i = 0; i < capacity; ++i)
            {
                GameObject newRound = GameObject.Instantiate(AmmoPrefab);
                newRound.transform.SetParent(MagazineContent.transform);
            }
        }

        public void UpdateMagazine(int remaining)
        {
            for (int i = 0; i < _magazineAmmo.Count; ++i)
            {
                GameObject round = _magazineAmmo[i].transform.Find("Round").gameObject;
                round.SetActive(i < remaining);
            }
        }
    }
}