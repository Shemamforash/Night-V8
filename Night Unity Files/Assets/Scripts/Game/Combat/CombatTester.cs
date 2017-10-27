using System.Collections.Generic;
using Game.Characters;
using Game.Gear.Weapons;
using Game.World;
using UnityEngine;

namespace Game.Combat
{
    public class CombatTester : MonoBehaviour
    {
        private DesolationCharacter character;
        private CombatScenario encounter;
        public int Size = 3;
        public bool ManualOnly;
        public bool Smg = true, Lmg = true, Rifle = true, Pistol = true, Shotgun = true;
    
        public void Start()
        {
            new DesolationCharacterManager();
            TraitLoader.LoadTraits();
            character = DesolationCharacterGenerator.GenerateCharacter();
            ((DesolationInventory)character.Inventory()).AddTestingResources();
            List<WeaponType> weaponsWanted = new List<WeaponType>();
            if(Smg) weaponsWanted.Add(WeaponType.SMG);
            if(Lmg) weaponsWanted.Add(WeaponType.LMG);
            if(Rifle) weaponsWanted.Add(WeaponType.Rifle);
            if(Pistol) weaponsWanted.Add(WeaponType.Shotgun);
            if(Shotgun) weaponsWanted.Add(WeaponType.Pistol);
            Weapon weapon = WeaponGenerator.GenerateWeapon(weaponsWanted, ManualOnly);
            character.Equip(weapon);
            weapon.Reload(character.Inventory());
            encounter = CombatScenario.Generate(Size);
            encounter.SetCharacter(character);
            CombatManager.EnterCombat(encounter);
        }
    }
}
