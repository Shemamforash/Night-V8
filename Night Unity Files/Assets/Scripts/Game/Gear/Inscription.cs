using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Combat.Player;
using Game.Gear.Weapons;
using Game.Global;
using NUnit.Framework;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Gear
{
    public class Inscription : GearItem
    {
        private static readonly List<InscriptionTemplate> _inscriptionTemplates = new List<InscriptionTemplate>();
        private static bool _readTemplates;
        private readonly AttributeModifier _modifier;
        private readonly InscriptionTemplate _template;
        private readonly int _inscriptionCost;

        private Inscription(InscriptionTemplate template, ItemQuality quality) : base("A " + QualityToInscription(quality) + " of " + template.Name, GearSubtype.Inscription, quality)
        {
            _template = template;
            _modifier = _template.GetModifier();
            float finalBonus = _modifier.FinalBonus();
            float rawBonus = _modifier.RawBonus();
            int tierModifier = (int) (quality + 1);
            finalBonus *= tierModifier;
            rawBonus *= tierModifier;
            _modifier.SetFinalBonus(finalBonus);
            _modifier.SetRawBonus(rawBonus);
            _inscriptionCost = ((int) quality + 1) * 5;
        }

        private bool IsCharacterAttribute(AttributeType attribute)
        {
            return attribute == AttributeType.Strength ||
                   attribute == AttributeType.Endurance ||
                   attribute == AttributeType.Willpower ||
                   attribute == AttributeType.Perception;
        }

        public void ApplyModifierToCharacter(Character character)
        {
            if (!IsCharacterAttribute(_template.AttributeTarget)) return;
            Player player = character as Player;
            if (player == null) return;
            CharacterAttribute attribute = player?.Attributes.Get(_template.AttributeTarget);
            Assert.IsNotNull(attribute);
            attribute.Max += _modifier.RawBonus();
            if (PlayerCombat.Instance == null) return;
            PlayerCombat.Instance.RecalculateAttributes();
        }

        public void ApplyModifierToWeapon(Weapon item)
        {
            if (IsCharacterAttribute(_template.AttributeTarget)) return;
            item.WeaponAttributes.Get(_template.AttributeTarget).AddModifier(_modifier);
        }

        public void RemoveModifierFromCharacter(Character character)
        {
            if (!IsCharacterAttribute(_template.AttributeTarget)) return;
            Player player = character as Player;
            if (player == null) return;
            CharacterAttribute attribute = player?.Attributes.Get(_template.AttributeTarget);
            Assert.IsNotNull(attribute);
            attribute.Max -= _modifier.RawBonus();
            if (PlayerCombat.Instance == null) return;
            PlayerCombat.Instance.RecalculateAttributes();
        }

        public void RemoveModifierFromWeapon(Weapon item)
        {
            item.WeaponAttributes.Get(_template.AttributeTarget).RemoveModifier(_modifier);
        }

        public static Inscription Generate()
        {
            ReadTemplates();
            int tier = WorldState.GenerateGearLevel();
            InscriptionTemplate randomTemplate = _inscriptionTemplates.RandomElement();
            return new Inscription(randomTemplate, (ItemQuality) tier);
        }

        private static void ReadTemplates()
        {
            if (_readTemplates) return;
            XmlNode inscriptions = Helper.OpenRootNode("Inscriptions");
            foreach (XmlNode inscriptionNode in Helper.GetNodesWithName(inscriptions, "Inscription"))
                new InscriptionTemplate(inscriptionNode);

            _readTemplates = true;
        }

        private static string QualityToInscription(ItemQuality quality)
        {
            switch (quality)
            {
                case ItemQuality.Dark:
                    return "Murmur";
                case ItemQuality.Dull:
                    return "Whisper";
                case ItemQuality.Glowing:
                    return "Cry";
                case ItemQuality.Shining:
                    return "Wail";
                case ItemQuality.Radiant:
                    return "Bellow";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override XmlNode Save(XmlNode root)
        {
            root = base.Save(root);
            root.CreateChild("InscriptionTemplate", _template.Name);
            return root;
        }

        public static Inscription LoadInscription(XmlNode root)
        {
            ReadTemplates();
            string templateString = root.StringFromNode("InscriptionTemplate");
            Debug.Log(templateString);
            InscriptionTemplate template = _inscriptionTemplates.First(t => t.Name == templateString);
            ItemQuality quality = (ItemQuality) root.IntFromNode("Quality");
            Inscription inscription = new Inscription(template, quality);
            inscription.Load(root);
            return inscription;
        }

        private class InscriptionTemplate
        {
            public readonly string Name;
            public readonly AttributeType AttributeTarget;
            private readonly bool _additive;
            private readonly float _modifierValue;

            public InscriptionTemplate(XmlNode inscriptionNode)
            {
                Name = inscriptionNode.StringFromNode("Name");
                AttributeTarget = Inventory.StringToAttributeType(inscriptionNode.StringFromNode("Attribute"));
                _modifierValue = inscriptionNode.FloatFromNode("Value");
                _additive = inscriptionNode.BoolFromNode("Additive");
                _inscriptionTemplates.Add(this);
            }

            public AttributeModifier GetModifier()
            {
                AttributeModifier modifier = new AttributeModifier();
                if (_additive) modifier.SetRawBonus(_modifierValue);
                else modifier.SetFinalBonus(_modifierValue);
                return modifier;
            }

            public string GetSummary(ItemQuality quality)
            {
                float scaledValue = _modifierValue * ((int) quality + 1);
                if (!_additive || AttributeTarget == AttributeType.DecayChance ||
                    AttributeTarget == AttributeType.BurnChance ||
                    AttributeTarget == AttributeType.SicknessChance)
                {
                    return "+" + (int) (scaledValue * 100) + "% " + AttributeTarget;
                }

                return "+" + scaledValue + " " + AttributeTarget;
            }
        }

        public override string GetSummary()
        {
            return _template.GetSummary(Quality());
        }

        public int InscriptionCost()
        {
            return _inscriptionCost;
        }

        public bool CanAfford()
        {
            return Inventory.GetResourceQuantity("Essence") >= _inscriptionCost;
        }
    }
}