using Game.Characters;
using Game.Combat.Enemies;
using Game.Gear.Weapons;
using UnityEngine;

namespace Game.Combat
{
    public class CombatTester : MonoBehaviour
    {
        private DesolationCharacter character;
        private Encounter encounter;
    
        private void Awake()
        {
            new DesolationCharacterManager();
            character = DesolationCharacterGenerator.GenerateCharacter();
            character.Equip(WeaponGenerator.GenerateWeapon());
        }
    }
}
