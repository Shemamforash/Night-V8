using System.Collections.Generic;
using System.Xml;
using Game.Characters;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Gear
{
    public class Inscription : InventoryItem
    {
        private static readonly List<InscriptionTemplate> _inscriptionTemplates = new List<InscriptionTemplate>();
        private static bool _readTemplates;
        private readonly AttributeModifier _modifier;
        private ItemQuality _quality;
        private readonly InscriptionTemplate _template;
        private readonly InscriptionTier _tier;

        private Inscription(InscriptionTemplate template, InscriptionTier tier) : base("A " + tier + " of " + template.Name, GameObjectType.Gear)
        {
            _template = template;
            _modifier = _template.GetModifier();
            _tier = tier;
            float finalBonus = _modifier.FinalBonus();
            float rawBonus = _modifier.RawBonus();
            int tierModifier = (int) (_tier + 1);
            finalBonus *= tierModifier;
            rawBonus *= tierModifier;
            _modifier.SetFinalBonus(finalBonus);
            _modifier.SetRawBonus(rawBonus);
        }

        public void ApplyModifier(Character character)
        {
            Player player = character as Player;
            player?.Attributes.Get(_template.AttributeTarget).AddModifier(_modifier);
        }

        public void ApplyModifier(Weapon item)
        {
            item.WeaponAttributes.Get(_template.AttributeTarget).AddModifier(_modifier);
        }

        public void RemoveModifier(Character character)
        {
            Player player = character as Player;
            player?.Attributes.Get(_template.AttributeTarget).RemoveModifier(_modifier);
        }

        public void RemoveModifier(Weapon item)
        {
            item.WeaponAttributes.Get(_template.AttributeTarget).RemoveModifier(_modifier);
        }

        public static Inscription Generate(int diff = -1)
        {
            ReadTemplates();
            InscriptionTier tier = (InscriptionTier) WorldState.GenerateGearLevel();
            InscriptionTemplate randomTemplate = Helper.RandomInList(_inscriptionTemplates);
            return new Inscription(randomTemplate, tier);
        }

        private static void ReadTemplates()
        {
            if (_readTemplates) return;
            XmlNode inscriptions = Helper.OpenRootNode("Inscriptions");
            foreach (XmlNode inscriptionNode in inscriptions.SelectNodes("Inscription"))
                new InscriptionTemplate(inscriptionNode);

            _readTemplates = true;
        }

        private enum InscriptionTier
        {
            Murmur,
            Whisper,
            Cry,
            Wail,
            Bellow
        }

        private class InscriptionTemplate
        {
            public readonly string Name;
            public readonly AttributeType AttributeTarget;
            private readonly bool _additive;
            private readonly float _modifierValue;

            public InscriptionTemplate(XmlNode inscriptionNode)
            {
                Name = inscriptionNode.SelectSingleNode("Name").InnerText;
                string attributeString = inscriptionNode.SelectSingleNode("Attribute").InnerText;
                AttributeTarget = Inventory.StringToAttributeType(attributeString);
                _modifierValue = float.Parse(inscriptionNode.SelectSingleNode("Value").InnerText);
                _additive = inscriptionNode.SelectSingleNode("Type").InnerText == "+";
                _inscriptionTemplates.Add(this);
            }

            public AttributeModifier GetModifier()
            {
                AttributeModifier modifier = new AttributeModifier();
                if (_additive) modifier.SetRawBonus(_modifierValue);
                else modifier.SetFinalBonus(_modifierValue);
                return modifier;
            }

            public string GetSummary()
            {
                if (_additive)
                {
                    return "+" + _modifierValue + " " + AttributeTarget;
                }

                return "+" + (int) (_modifierValue * 100) + "% " + AttributeTarget;
            }
        }

        public string GetSummary()
        {
            return Name + "\n" + _template.GetSummary();
        }
    }
}