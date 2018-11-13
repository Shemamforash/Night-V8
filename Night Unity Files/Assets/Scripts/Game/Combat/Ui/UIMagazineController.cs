﻿using System.Collections.Generic;
using Game.Combat.Player;
using Game.Gear.Weapons;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Ui
{
    public class UIMagazineController : MonoBehaviour
    {
        private static GameObject _ammoPrefab;
        private static Transform _magazineContent;
        private static readonly List<Ammo> MagazineAmmo = new List<Ammo>();
        private static int _capacity;
        private static BaseWeaponBehaviour _weapon;
        private static bool _empty;
        private static int _oldCapacity;

        public void Awake()
        {
            _magazineContent = transform;
            _ammoPrefab = Resources.Load("Prefabs/Combat/Ammo Prefab") as GameObject;
        }

        public static void UpdateMagazineUi()
        {
            if (_weapon.Empty()) EmptyMagazine();
            else UpdateMagazine();
        }

        private static void UpdateMagazine(int remaining = -1)
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
            _oldCapacity = 0;
        }

        public static bool UpdateReloadTime(float time)
        {
            bool increased = false;
            int newCapacity = (int) (_capacity * time);
            if (_oldCapacity != newCapacity) increased = true;
            UpdateMagazine(newCapacity);
            _oldCapacity = newCapacity;
            return increased;
        }

        public static void SetWeapon(BaseWeaponBehaviour weapon)
        {
            _weapon = weapon;
            if (weapon == null) return;
            _capacity = weapon.Capacity();
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