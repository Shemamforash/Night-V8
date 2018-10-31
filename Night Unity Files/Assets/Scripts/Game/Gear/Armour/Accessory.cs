using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Combat.Player;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using Random = UnityEngine.Random;

namespace Game.Gear.Armour
{
    public class Accessory : GearItem
    {
        private static readonly List<AccessoryTemplate> _accessoryTemplates = new List<AccessoryTemplate>();
        private static bool _loaded;
        private readonly AccessoryTemplate _template;
        private AttributeModifier modifier;

        private Accessory(AccessoryTemplate template, ItemQuality itemQuality) : base(template.Name, GearSubtype.Accessory, itemQuality)
        {
            _template = template;
        }

        public override string GetSummary()
        {
            return Name;
        }

        public override void Equip(Character character)
        {
            base.Equip(character);
            (character as Player)?.ApplyModifier(_template.TargetAttribute, modifier);
            ApplyToWeapon(character.EquippedWeapon);
            if (PlayerCombat.Instance == null) return;
            PlayerCombat.Instance.RecalculateAttributes();
        }

        public override void Unequip()
        {
            base.Unequip();
            (EquippedCharacter as Player)?.RemoveModifier(_template.TargetAttribute, modifier);
            RemoveFromWeapon(EquippedCharacter.EquippedWeapon);
            if (PlayerCombat.Instance == null) return;
            PlayerCombat.Instance.RecalculateAttributes();
        }

        private static void ReadTemplates()
        {
            if (_loaded) return;
            XmlNode root = Helper.OpenRootNode("Gear", "GearList");
            foreach (XmlNode accessoryNode in Helper.GetNodesWithName(root, "Gear"))
                new AccessoryTemplate(accessoryNode);
            _loaded = true;
        }

        public static Accessory Generate()
        {
            ReadTemplates();
            AccessoryTemplate randomTemplate = _accessoryTemplates[Random.Range(0, _accessoryTemplates.Count)];

            ItemQuality quality = (ItemQuality) WorldState.GenerateGearLevel();

            Accessory accessory = new Accessory(randomTemplate, quality);
            AttributeModifier modifier = new AttributeModifier();
            modifier = randomTemplate.GetModifier();
            float rawBonus = modifier.RawBonus() * (int) quality;
            float finalBonus = modifier.FinalBonus() * (int) quality;
            modifier.SetFinalBonus(finalBonus);
            modifier.SetRawBonus(rawBonus);
            accessory.modifier = modifier;
            return accessory;
        }

        public override XmlNode Save(XmlNode doc)
        {
            doc = base.Save(doc);
            doc.CreateChild("AccessoryTemplate", _template.Name);
            return doc;
        }

        private class AccessoryTemplate
        {
            public readonly string Name;
            public readonly AttributeType TargetAttribute;
            private readonly float _modifierValue;

            public AccessoryTemplate(XmlNode accessoryNode)
            {
                Name = accessoryNode.StringFromNode("Name");
                TargetAttribute = Inventory.StringToAttributeType(accessoryNode.StringFromNode("Attribute"));
                _modifierValue = accessoryNode.FloatFromNode("Bonus");
                _accessoryTemplates.Add(this);
            }

            public AttributeModifier GetModifier()
            {
                AttributeModifier modifier = new AttributeModifier();
                modifier.SetFinalBonus(_modifierValue);
                return modifier;
            }
        }

        public static Accessory LoadAccessory(XmlNode accessoryNode)
        {
            ReadTemplates();
            string templateString = accessoryNode.StringFromNode("AccessoryTemplate");
            AccessoryTemplate template = _accessoryTemplates.First(t => t.Name == templateString);
            Accessory accessory = new Accessory(template, ItemQuality.Dark);
            accessory.Load(accessoryNode);
            return accessory;
        }

        public void ApplyToWeapon(Weapon weapon)
        {
            weapon?.ApplyModifier(_template.TargetAttribute, modifier);
        }

        public void RemoveFromWeapon(Weapon weapon)
        {
            weapon?.RemoveModifier(_template.TargetAttribute, modifier);
        }
    }
}