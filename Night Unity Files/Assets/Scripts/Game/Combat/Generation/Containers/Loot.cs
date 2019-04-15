using Game.Gear;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using InventorySystem;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class Loot : ContainerController
    {
        private static Sprite _accessorySprite, _weaponSprite, _inscriptionSprite;

        public Loot(Vector2 position) : base(position)
        {
            if (_accessorySprite == null) _accessorySprite = Resources.Load<Sprite>("Images/Container Symbols/Accessory");
            if (_weaponSprite == null) _weaponSprite = Resources.Load<Sprite>("Images/Container Symbols/Weapon");
            if (_inscriptionSprite == null) _inscriptionSprite = Resources.Load<Sprite>("Images/Container Symbols/Inscription");
        }

        public void SetResource(ResourceItem item)
        {
            Item = item;
            if (item.Template?.ResourceType == ResourceType.Meat) Sprite = ResourceTemplate.GetSprite("Meat");
        }

        public void SetItem(GearItem item)
        {
            Item = item;
            switch (item)
            {
                case Accessory _:
                    Sprite = _accessorySprite;
                    break;
                case Weapon _:
                    Sprite = _weaponSprite;
                    break;
                case Inscription _:
                    Sprite = _inscriptionSprite;
                    break;
            }
        }

        protected override string GetLogText()
        {
            switch (Item)
            {
                case Weapon _:
                    return "Found a Weapon";
                case Accessory _:
                    return "Found an Accessory";
                case Inscription _:
                    return "Found an Inscription";
                case ResourceItem item:
                    return "Found " + item.Name;
            }

            return "";
        }
    }
}