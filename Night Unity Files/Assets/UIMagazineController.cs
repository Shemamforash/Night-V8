using System.Collections.Generic;
using Game.Combat;
using Game.Gear.Weapons;
using SamsHelper;
using TMPro;
using UnityEngine;

namespace Assets
{
    public class UIMagazineController : MonoBehaviour
    {
        private static GameObject _ammoPrefab;
        private static Transform _magazineContent;
        private static TextMeshProUGUI _ammoText;
        private static readonly List<Ammo> MagazineAmmo = new List<Ammo>();
        private static int _capacity;
        private static Weapon _weapon;
        private static bool _empty;
        private const float EmptyMagazinePause = 0.2f;

        public void Awake()
        {
            _magazineContent = transform.Find("Magazine");
            _ammoPrefab = Resources.Load("Prefabs/Combat/Ammo Prefab") as GameObject;
            _ammoText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Ammo");
        }

        private class Ammo
        {
            private readonly GameObject _round, _ammoObject;

            public Ammo(GameObject ammoObject)
            {
                _ammoObject = ammoObject;
                _round = _ammoObject.transform.Find("Round").gameObject;
            }

            public void SetUnspent(bool unspent)
            {
                _round.SetActive(unspent);
            }

            public void Destroy()
            {
                GameObject.Destroy(_ammoObject);
            }
        }

        public static void UpdateMagazine(int remaining = -1)
        {
            if (remaining == -1) remaining = _weapon.GetRemainingAmmo();
            for (int i = 0; i < MagazineAmmo.Count; ++i)
            {
                MagazineAmmo[i].SetUnspent(i < remaining);
            }
            SetMessage(CombatManager.Player().Weapon().GetRemainingMagazines() + " mags");
            _empty = false;
        }

        public static void SetMessage(string message)
        {
            _ammoText.text = message;
        }

        public static void EmptyMagazine()
        {
            if (_empty) return;
            foreach (Ammo ammo in MagazineAmmo)
            {
                ammo.SetUnspent(false);
            }
            _empty = true;
        }

        public static void UpdateReloadTime(float time)
        {
            if (time < EmptyMagazinePause)
            {
                EmptyMagazine();
                return;
            }
            int newCapacity = (int) (_capacity * (1- time / (1 - EmptyMagazinePause)));
            UpdateMagazine(newCapacity);
        }

        public static void SetWeapon(Weapon weapon)
        {
            _weapon = weapon;
            _capacity = (int) weapon.WeaponAttributes.Capacity.CurrentValue();
            MagazineAmmo.ForEach(a => a.Destroy());
            MagazineAmmo.Clear();
            for (int i = 0; i < _capacity; ++i)
            {
                Ammo newRound = new Ammo(Helper.InstantiateUiObject(_ammoPrefab, _magazineContent));
                MagazineAmmo.Add(newRound);
            }
            UpdateMagazine();
        }
    }
}