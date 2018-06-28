using Game.Characters;
using Game.Exploration.Regions;
using Game.Gear;
using Game.Gear.Weapons;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class CombatTester : MonoBehaviour
    {
        private static Characters.Player _playerCharacter;
        private static Region _encounter;
        private static CombatTester _instance;
        private int currentDanger = 0;
        public int Difficulty = 3;

        [Range(1, 20)] public int Durability;

        public WeaponClassType WeaponClass;

        public void Start()
        {
//            new CharacterManager();
//            _instance = this;
//            RestartCombat();
        }

        private static void RestartCombat()
        {
            _playerCharacter = CharacterManager.GenerateRandomCharacter();
            _playerCharacter.Inventory().AddTestingResources(5);
            Weapon weapon = WeaponGenerator.GenerateWeapon(ItemQuality.Radiant, _instance.WeaponClass);
            weapon.WeaponAttributes.SetDurability(_instance.Durability);
            _playerCharacter.EquipWeapon(weapon);
            CharacterManager.SelectedCharacter = _playerCharacter;
            Region node = new Region();
//            node.SetRegionType(RegionType.Danger);
//            node.SetRegionType(RegionType.Nightmare);
            node.SetRegionType(RegionType.Animal);
            _playerCharacter.TravelAction.SetCurrentNode(node);
            _playerCharacter.Inventory().Print();
        }
    }
}