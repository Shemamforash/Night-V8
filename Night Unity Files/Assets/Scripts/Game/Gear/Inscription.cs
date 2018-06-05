using System;
using System.Collections.Generic;
using System.Xml;
using Game.Characters;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Gear
{
    public class Inscription : GearItem
    {
        private static readonly List<InscriptionTemplate> _inscriptionTemplates = new List<InscriptionTemplate>();
        private static bool _readTemplates;
        private AttributeModifier _modifier;
        private ItemQuality _quality;
        private InscriptionTemplate _template;

        private Inscription(InscriptionTemplate template, ItemQuality quality) : base(template.Name, 0.25f, GearSubtype.Inscription, quality)
        {
            _template = template;
        }

        private static void ReadTemplates()
        {
            if (_readTemplates) return;
            TextAsset inscriptionFile = Resources.Load<TextAsset>("XML/Inscriptions");
            XmlDocument inscriptionXml = new XmlDocument();
            inscriptionXml.LoadXml(inscriptionFile.text);
            XmlNode inscriptions = inscriptionXml.SelectSingleNode("//Inscriptions");
            foreach (XmlNode inscriptionNode in inscriptions.SelectNodes("Inscription"))
            {
                string inscriptionName = inscriptionNode.SelectSingleNode("Name").InnerText;
                InscriptionTemplate inscriptionTemplate = new InscriptionTemplate(inscriptionName);
                foreach (AttributeType attributeType in Enum.GetValues(typeof(AttributeType)))
                {
                    XmlNode attributeNode = inscriptionNode.SelectSingleNode(attributeType.ToString());
                    if (attributeNode == null) continue;
                    string attributeText = attributeNode.InnerText;
                    char prefix = attributeText[0];
                    float value = float.Parse(attributeText.Substring(1));
                    if (prefix == 'x' && value != 1)
                    {
                        AttributeModifier modifier = new AttributeModifier();
                        modifier.SetFinalBonus(value);
                        inscriptionTemplate.SetModifier(modifier);
                    }
                    else if (prefix == '+' && value != 0)
                    {
                        AttributeModifier modifier = new AttributeModifier();
                        modifier.SetFinalBonus(value);
                        inscriptionTemplate.SetModifier(modifier);
                    }
                }
            }

            _readTemplates = true;
        }

        public static Inscription GenerateInscription(ItemQuality quality)
        {
            ReadTemplates();
            InscriptionTemplate randomTemplate = _inscriptionTemplates[Random.Range(0, _inscriptionTemplates.Count)];
            return new Inscription(randomTemplate, quality);
        }

        public override string GetSummary()
        {
            return "Inscription summary";
        }

        public class InscriptionTemplate
        {
            public readonly string Name;
            private AttributeModifier _modifierOne;
            private AttributeModifier _modifierTwo;

            public InscriptionTemplate(string name)
            {
                Name = name;
                _inscriptionTemplates.Add(this);
            }

            public void SetModifier(AttributeModifier modifier)
            {
                if (_modifierOne == null)
                {
                    _modifierOne = modifier;
                    return;
                }

                if (_modifierTwo != null) throw new Exceptions.InscriptionModificationException(Name);
                _modifierTwo = modifier;
            }
        }
    }
}