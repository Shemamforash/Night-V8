using System.Collections.Generic;
using Game.Characters;
using Game.Gear.Weapons;
using Game.World;
using UnityEngine;

namespace Game.Combat
{
    public class CombatTester : MonoBehaviour
    {
        private static Player _playerCharacter;
        private static CombatScenario _encounter;
        public int Size = 3;
        public bool ManualOnly;
        public bool Smg = true, Lmg = true, Rifle = true, Pistol = true, Shotgun = true;
        private static CombatTester _instance;
        
        public void Start()
        {
            new CharacterManager();
            TraitLoader.LoadTraits();
            _instance = this;
            RestartCombat();
        }

        public static void RestartCombat()
        {
            _playerCharacter = CharacterGenerator.GenerateCharacter();
            _playerCharacter.Inventory().AddTestingResources();
            List<WeaponType> weaponsWanted = new List<WeaponType>();
            if(_instance.Smg) weaponsWanted.Add(WeaponType.SMG);
            if(_instance.Lmg) weaponsWanted.Add(WeaponType.LMG);
            if(_instance.Rifle) weaponsWanted.Add(WeaponType.Rifle);
            if(_instance.Pistol) weaponsWanted.Add(WeaponType.Pistol);
            if(_instance.Shotgun) weaponsWanted.Add(WeaponType.Shotgun);
            Weapon weapon = WeaponGenerator.GenerateWeapon(weaponsWanted, _instance.ManualOnly);
            _playerCharacter.Equip(weapon);
            weapon.Reload(_playerCharacter.Inventory());
            
            _encounter = CombatScenario.Generate(_instance.Size);
            _encounter.SetCharacter(_playerCharacter);
            CombatManager.EnterCombat(_encounter);
        }
    }
}
