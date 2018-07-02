﻿using System.Collections.Generic;
using System.Xml;
using Game.Characters;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

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

            int difficulty = Mathf.FloorToInt(WorldState.Difficulty() / 10f);
            int difficultyMin = difficulty - 1;
            if (difficultyMin < 0) difficultyMin = 0;
            else if (difficultyMin > 4) difficultyMin = 4;
            int difficultyMax = difficulty + 1;
            if (difficultyMax > 4) difficultyMax = 4;
            
            InscriptionTier tier = (InscriptionTier) difficulty;
            InscriptionTemplate randomTemplate = Helper.RandomInList(_inscriptionTemplates);
            return new Inscription(randomTemplate, tier);
        }

        private static void ReadTemplates()
        {
            if (_readTemplates) return;
            XmlNode inscriptions = Helper.OpenRootNode("Inscriptions");
            foreach (XmlNode inscriptionNode in inscriptions.SelectNodes("Inscription"))
            {
                string inscriptionName = inscriptionNode.SelectSingleNode("Name").InnerText;
                string attributeString = inscriptionNode.SelectSingleNode("Attribute").InnerText;
                AttributeType attributeType = Inventory.StringToAttributeType(attributeString);
                float value = float.Parse(inscriptionNode.SelectSingleNode("Value").InnerText);
                bool additive = inscriptionNode.SelectSingleNode("Type").InnerText == "+";
                InscriptionTemplate inscriptionTemplate = new InscriptionTemplate(inscriptionName, attributeType, additive, value);
                _inscriptionTemplates.Add(inscriptionTemplate);
            }

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

            public InscriptionTemplate(string name, AttributeType attributeType, bool additive, float modifierValue)
            {
                Name = name;
                AttributeTarget = attributeType;
                _inscriptionTemplates.Add(this);
                _additive = additive;
                _modifierValue = modifierValue;
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