using System.Collections.Generic;
using Game.Characters;
using Game.Gear.Weapons;
using Game.World;
using UnityEngine;

namespace Game.Combat
{
    public class CombatTester : MonoBehaviour
    {
        private Player _playerCharacter;
        private CombatScenario _encounter;
        public int Size = 3;
        public bool ManualOnly;
        public bool Smg = true, Lmg = true, Rifle = true, Pistol = true, Shotgun = true;
    
        public void Start()
        {
            new CharacterManager();
            TraitLoader.LoadTraits();
            _playerCharacter = CharacterGenerator.GenerateCharacter();
            ((DesolationInventory)_playerCharacter.Inventory()).AddTestingResources();
            List<WeaponType> weaponsWanted = new List<WeaponType>();
            if(Smg) weaponsWanted.Add(WeaponType.SMG);
            if(Lmg) weaponsWanted.Add(WeaponType.LMG);
            if(Rifle) weaponsWanted.Add(WeaponType.Rifle);
            if(Pistol) weaponsWanted.Add(WeaponType.Shotgun);
            if(Shotgun) weaponsWanted.Add(WeaponType.Pistol);
            Weapon weapon = WeaponGenerator.GenerateWeapon(weaponsWanted, ManualOnly);
            _playerCharacter.Equip(weapon);
            weapon.Reload(_playerCharacter.Inventory());
            _encounter = CombatScenario.Generate(Size);
            _encounter.SetCharacter(_playerCharacter);
            CombatManager.EnterCombat(_encounter);
        }
    }
}
