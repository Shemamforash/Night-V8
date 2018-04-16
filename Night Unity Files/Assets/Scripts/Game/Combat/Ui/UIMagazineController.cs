﻿using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Gear.Weapons;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;

namespace Game.Combat.Ui
{
    public class UIMagazineController : MonoBehaviour
    {
        private static GameObject _ammoPrefab;
        private static Transform _magazineContent;
        private static readonly List<Ammo> MagazineAmmo = new List<Ammo>();
        private static int _capacity;
        private static Weapon _weapon;
        private static bool _empty;

        public void Awake()
        {
            _magazineContent = transform;
            _ammoPrefab = Resources.Load("Prefabs/Combat/Ammo Prefab") as GameObject;
        }

        public static void UpdateMagazine(int remaining = -1)
        {
            if (remaining == -1) remaining = _weapon.GetRemainingAmmo();
            for (int i = 0; i < MagazineAmmo.Count; ++i) MagazineAmmo[i].SetUnspent(i < remaining);
            _empty = false;
        }


        public static void EmptyMagazine()
        {
            if (_empty) return;
            foreach (Ammo ammo in MagazineAmmo) ammo.SetUnspent(false);
            _empty = true;
        }

        public static void UpdateReloadTime(float time)
        {
            int newCapacity = (int) (_capacity * time);
            UpdateMagazine(newCapacity);
        }

        public static void SetWeapon(Weapon weapon)
        {
            _weapon = weapon;
            if (weapon == null) return;
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

        private class Ammo
        {
            private readonly GameObject _round, _ammoObject;

            public Ammo(GameObject ammoObject)
            {
                Vector3 position = ammoObject.transform.position;
                position.z = 0;
                ammoObject.transform.position = position;
                _ammoObject = ammoObject;
                _round = _ammoObject.transform.Find("Round").gameObject;
            }

            public void SetUnspent(bool unspent)
            {
                _round.SetActive(unspent);
            }

            public void Destroy()
            {
                Object.Destroy(_ammoObject);
            }
        }
    }
}