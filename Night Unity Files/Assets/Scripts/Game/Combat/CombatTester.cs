using Game.Characters;
using Game.Gear.Weapons;
using UnityEngine;

namespace Game.Combat
{
    public class CombatTester : MonoBehaviour
    {
        private DesolationCharacter character;
        private CombatScenario encounter;
        public int Size = 3;
    
        public void Start()
        {
            new DesolationCharacterManager();
            TraitLoader.LoadTraits();
            character = DesolationCharacterGenerator.GenerateCharacter();
            character.Equip(WeaponGenerator.GenerateWeapon());
            encounter = CombatScenario.Generate(Size);
            encounter.SetCharacter(character);
            CombatManager.EnterCombat(encounter);
        }
    }
}
