﻿using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Generation;
using Game.Gear;
using Game.Gear.Weapons;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class CombatTester : MonoBehaviour
    {
        private static Characters.Player _playerCharacter;
        private static CombatScenario _encounter;
        private static CombatTester _instance;
        private int currentDanger = 0;
        public int Difficulty = 3;

        [Range(1, 20)] public int Durability;

        public bool ManualOnly;
        public bool Smg = true, Lmg = true, Rifle = true, Pistol = true, Shotgun = true;

        public void Start()
        {
            new CharacterManager();
            _instance = this;
            RestartCombat();
        }

        public static void RestartCombat()
        {
            _playerCharacter = CharacterManager.GenerateRandomCharacter();
            _playerCharacter.Inventory().AddTestingResources();
            List<WeaponType> weaponsWanted = new List<WeaponType>();
            if (_instance.Smg) weaponsWanted.Add(WeaponType.SMG);
            if (_instance.Lmg) weaponsWanted.Add(WeaponType.LMG);
            if (_instance.Rifle) weaponsWanted.Add(WeaponType.Rifle);
            if (_instance.Pistol) weaponsWanted.Add(WeaponType.Pistol);
            if (_instance.Shotgun) weaponsWanted.Add(WeaponType.Shotgun);
            Weapon weapon = WeaponGenerator.GenerateWeapon(ItemQuality.Shining, weaponsWanted);
            weapon.WeaponAttributes.SetDurability(_instance.Durability);
            Debug.Log(weapon.WeaponAttributes.Print());
            _playerCharacter.EquipWeapon(weapon);
            weapon.Reload(_playerCharacter.Inventory());

            _encounter = CombatScenario.Generate(_instance.Difficulty, 8);
            CombatManager.EnterCombat(_playerCharacter, _encounter);
        }
    }
}