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
        private AttributeModifier _modifier;
        private ItemQuality _quality;
        private InscriptionTemplate _template;
        private static readonly List<AttributeType> _attributeTypes = new List<AttributeType>();
        private InscriptionTier _tier;

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
            if (player == null) return;
            switch (_template.AttributeTarget)
            {
                case AttributeType.Strength:
                    player.Attributes.Strength.AddModifier(_modifier);
                    break;
                case AttributeType.Endurance:
                    player.Attributes.Endurance.AddModifier(_modifier);
                    break;
                case AttributeType.Perception:
                    player.Attributes.Perception.AddModifier(_modifier);
                    break;
                case AttributeType.Willpower:
                    player.Attributes.Willpower.AddModifier(_modifier);
                    break;
            }
        }

        public void ApplyModifier(Weapon item)
        {
            switch (_template.AttributeTarget)
            {
                case AttributeType.Damage:
                    item.WeaponAttributes.Damage.AddModifier(_modifier);
                    break;
                case AttributeType.Accuracy:
                    item.WeaponAttributes.Accuracy.AddModifier(_modifier);
                    break;
                case AttributeType.FireRate:
                    item.WeaponAttributes.FireRate.AddModifier(_modifier);
                    break;
                case AttributeType.ReloadSpeed:
                    item.WeaponAttributes.ReloadSpeed.AddModifier(_modifier);
                    break;
                case AttributeType.Handling:
                    item.WeaponAttributes.Handling.AddModifier(_modifier);
                    break;
                case AttributeType.Pellets:
                    item.WeaponAttributes.Pellets.AddModifier(_modifier);
                    break;
                case AttributeType.Capacity:
                    item.WeaponAttributes.Capacity.AddModifier(_modifier);
                    break;
                case AttributeType.SicknessChance:
                    item.WeaponAttributes.SicknessChance.AddModifier(_modifier);
                    break;
                case AttributeType.DecayChance:
                    item.WeaponAttributes.BleedChance.AddModifier(_modifier);
                    break;
                case AttributeType.BurnChance:
                    item.WeaponAttributes.BurnChance.AddModifier(_modifier);
                    break;
            }
        }

        public void RemoveModifier(Character character)
        {
            Player player = character as Player;
            if (player == null) return;
            switch (_template.AttributeTarget)
            {
                case AttributeType.Strength:
                    player.Attributes.Strength.RemoveModifier(_modifier);
                    break;
                case AttributeType.Endurance:
                    player.Attributes.Endurance.RemoveModifier(_modifier);
                    break;
                case AttributeType.Perception:
                    player.Attributes.Perception.RemoveModifier(_modifier);
                    break;
                case AttributeType.Willpower:
                    player.Attributes.Willpower.RemoveModifier(_modifier);
                    break;
            }
        }

        public void RemoveModifier(Weapon item)
        {
            switch (_template.AttributeTarget)
            {
                case AttributeType.Damage:
                    item.WeaponAttributes.Damage.RemoveModifier(_modifier);
                    break;
                case AttributeType.Accuracy:
                    item.WeaponAttributes.Accuracy.RemoveModifier(_modifier);
                    break;
                case AttributeType.FireRate:
                    item.WeaponAttributes.FireRate.RemoveModifier(_modifier);
                    break;
                case AttributeType.ReloadSpeed:
                    item.WeaponAttributes.ReloadSpeed.RemoveModifier(_modifier);
                    break;
                case AttributeType.Handling:
                    item.WeaponAttributes.Handling.RemoveModifier(_modifier);
                    break;
                case AttributeType.Pellets:
                    item.WeaponAttributes.Pellets.RemoveModifier(_modifier);
                    break;
                case AttributeType.Capacity:
                    item.WeaponAttributes.Capacity.RemoveModifier(_modifier);
                    break;
                case AttributeType.SicknessChance:
                    item.WeaponAttributes.SicknessChance.RemoveModifier(_modifier);
                    break;
                case AttributeType.DecayChance:
                    item.WeaponAttributes.BleedChance.RemoveModifier(_modifier);
                    break;
                case AttributeType.BurnChance:
                    item.WeaponAttributes.BurnChance.RemoveModifier(_modifier);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            if (_attributeTypes.Count == 0)
            {
                foreach (AttributeType attributeType in Enum.GetValues(typeof(AttributeType))) _attributeTypes.Add(attributeType);
            }

            TextAsset inscriptionFile = Resources.Load<TextAsset>("XML/Inscriptions");
            XmlDocument inscriptionXml = new XmlDocument();
            inscriptionXml.LoadXml(inscriptionFile.text);
            XmlNode inscriptions = inscriptionXml.SelectSingleNode("//Inscriptions");
            foreach (XmlNode inscriptionNode in inscriptions.SelectNodes("Inscription"))
            {
                string inscriptionName = inscriptionNode.SelectSingleNode("Name").InnerText;
                foreach (AttributeType attributeType in _attributeTypes)
                {
                    XmlNode attributeNode = inscriptionNode.SelectSingleNode(attributeType.ToString());
                    if (attributeNode == null) continue;

                    string attributeText = attributeNode.InnerText;
                    bool additive = attributeText[0] == '+';
                    float value = float.Parse(attributeText.Substring(1));
                    InscriptionTemplate inscriptionTemplate = new InscriptionTemplate(inscriptionName, attributeType, additive, value);
                    _inscriptionTemplates.Add(inscriptionTemplate);
                    break;
                }
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