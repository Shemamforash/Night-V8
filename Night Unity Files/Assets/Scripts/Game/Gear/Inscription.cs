using System;
using System.Collections.Generic;
using System.Xml;
using Game.Characters;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper;
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

        private Inscription(InscriptionTemplate template, InscriptionTier tier) : base(template.Name, GameObjectType.Gear)
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
            Debug.Log(GetSummary());
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
            int difficulty = diff == -1 ? WorldState.Difficulty() : diff;
            float maxRange = 7 * 5;
            float range = difficulty / maxRange * 2 + 1.5f;
            float rand = Random.Range(0, range);
            InscriptionTier tier = InscriptionTier.Bellow;
            if (rand < 1)
            {
                tier = InscriptionTier.Whisper;
            }
            else if (rand < 2)
            {
                tier = InscriptionTier.Wail;
            }

            InscriptionTemplate randomTemplate = Helper.RandomInList(_inscriptionTemplates);
            return new Inscription(randomTemplate, tier);
        }

        public static void Test()
        {
            string results = "";
            for (int i = 0; i <= 10; ++i)
            {
                int diff = i * 10;
                int cnt = 1000;
                int whisperCount = 0;
                int wailCount = 0;
                int bellowCount = 0;
                for (int j = 0; j < cnt; ++j)
                {
                    Inscription inscription = Generate(diff);
                    switch (inscription._tier)
                    {
                        case InscriptionTier.Whisper:
                            ++whisperCount;
                            break;
                        case InscriptionTier.Wail:
                            ++wailCount;
                            break;
                        case InscriptionTier.Bellow:
                            ++bellowCount;
                            break;
                    }
                }

                results += "difficulty" + diff + "---   whisper: " + Mathf.Round((float) whisperCount / cnt * 100) + "%   wail: " + Mathf.Round((float) wailCount / cnt * 100) + "%   bellow:" + Mathf
                               .Round((float) bellowCount / cnt * 100) + "%\n";
            }

            Debug.Log(results);
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

        public enum InscriptionTier
        {
            Whisper,
            Wail,
            Bellow
        }

        public string GetSummary()
        {
            return "A " + _tier + " of " + _template.Name;
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
        }
    }
}