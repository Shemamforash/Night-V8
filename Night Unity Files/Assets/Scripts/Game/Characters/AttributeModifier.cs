﻿using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;

namespace Game.Characters
{
    public class AttributeModifier
    {
        private int _depth;
        private float _finalBonus, _rawBonus;
        public readonly List<CharacterAttribute> TargetAttributes = new List<CharacterAttribute>();

        public AttributeModifier(int depth = 0)
        {
            _depth = depth;
        }

        public int Depth() => _depth;
        
        public void SetFinalBonus(float finalBonus)
        {
            _finalBonus = finalBonus;
            UpdateTargetAttributes();
        }

        public void SetRawBonus(float rawBonus)
        {
            _rawBonus = rawBonus;
            UpdateTargetAttributes();
        }

        private void UpdateTargetAttributes()
        {
            TargetAttributes.ForEach(t => t.Recalculate());
        }

        private string ModifierToString(float modifier)
        {
            string modifierString = "";
            if (modifier != 0) modifierString = modifier.AddSignPrefix();
            return modifierString;
        }

        public string RawBonusToString()
        {
            string rawBonusString = ModifierToString(_rawBonus);
            return rawBonusString; // + " " + _targetAttribute;
        }

        public string FinalBonusToString()
        {
            if (_finalBonus == 0) return "";
            string finalBonusString = ModifierToString((_finalBonus - 1) * 100);
            return finalBonusString; // + " " + _targetAttribute;
        }

        public float FinalBonus()
        {
            return _finalBonus;
        }

        public float RawBonus()
        {
            return _rawBonus;
        }

        public void Save(XmlNode doc)
        {
            doc.CreateChild("FinalBonus", _finalBonus);
            doc.CreateChild("RawBonus", _rawBonus);
        }

        public static AttributeModifier Load(XmlNode modifierNode)
        {
            AttributeModifier modifier = new AttributeModifier();
            modifier.SetFinalBonus(modifierNode.FloatFromNode("FinalBonus"));
            modifier.SetRawBonus(modifierNode.FloatFromNode("RawBonus"));
            return modifier;
        }
    }
}