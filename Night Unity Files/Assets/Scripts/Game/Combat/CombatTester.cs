using Game.Characters;
using Game.Gear.Weapons;
using Game.World;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Combat
{
    public class CombatTester : MonoBehaviour
    {
        private DesolationCharacter character;
        private CombatScenario encounter;
        public int Size = 3;
        public bool ManualOnly;
    
        public void Start()
        {
            new DesolationCharacterManager();
            WorldState.AddTestingResources();
            TraitLoader.LoadTraits();
            character = DesolationCharacterGenerator.GenerateCharacter();
            character.Equip(WeaponGenerator.GenerateWeapon(ManualOnly));
            encounter = CombatScenario.Generate(Size);
            encounter.SetCharacter(character);
            CombatManager.EnterCombat(encounter);
        }
    }
}
