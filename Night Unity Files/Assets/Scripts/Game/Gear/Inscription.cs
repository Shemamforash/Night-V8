﻿using System;
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

namespace Game.Gear
{
    public class Inscription : GearItem
    {
        private static readonly List<InscriptionTemplate> _inscriptionTemplates = new List<InscriptionTemplate>();
        private static bool _readTemplates;
        private readonly AttributeModifier _modifier;
        private readonly InscriptionTemplate _template;
        private readonly int _inscriptionCost;

        private Inscription(InscriptionTemplate template, ItemQuality quality) : base("A " + QualityToInscription(quality) + " of " + template.Name, quality)
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
            _inscriptionCost = (int) quality + 1;
        }

        public AttributeModifier Modifier()
        {
            return _modifier;
        }

        public AttributeType Target()
        {
            return _template.AttributeTarget;
        }

        public string TemplateName()
        {
            return _template.Name;
        }

        public static Inscription Generate()
        {
            ItemQuality tier = WorldState.GenerateGearLevel();
            return Generate(tier);
        }

        public static Inscription Generate(ItemQuality tier)
        {
            ReadTemplates();
            InscriptionTemplate randomTemplate = _inscriptionTemplates.RandomElement();
            return new Inscription(randomTemplate, tier);
        }

        private static void ReadTemplates()
        {
            if (_readTemplates) return;
            XmlNode inscriptions = Helper.OpenRootNode("Inscriptions");
            foreach (XmlNode inscriptionNode in inscriptions.GetNodesWithName("Inscription"))
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
            root = root.CreateChild("Inscription");
            base.Save(root);
            root.CreateChild("InscriptionTemplate", _template.Name);
            return root;
        }

        public static Inscription LoadInscription(XmlNode root)
        {
            ReadTemplates();
            string templateString = root.StringFromNode("InscriptionTemplate");
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
                string attributeName = AttributeTarget.AttributeToDisplayString();
                string prefix = "";
                if (scaledValue > 0) prefix = "+";
                if (!_additive) return prefix + (int) (scaledValue * 100) + "% " + attributeName;
                return prefix + (int) scaledValue + " " + attributeName;
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

        protected override void CalculateDismantleRewards()
        {
            base.CalculateDismantleRewards();
            int quality = (int) Quality() + 1;
            AddReward("Essence", 5 * quality);
            if (Helper.RollDie(0, 6)) AddReward("Radiance", 1);
        }
    }
}