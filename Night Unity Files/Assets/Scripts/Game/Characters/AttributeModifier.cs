using System.Collections.Generic;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;

namespace Game.Characters
{
    public class AttributeModifier
    {
        private float _finalBonus, _rawBonus;
        private List<CharacterAttribute> _targetAttributes = new List<CharacterAttribute>();

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
            _targetAttributes.ForEach(t => t.Recalculate());
        }

        public void AddTargetAttribute(CharacterAttribute c)
        {
            _targetAttributes.Add(c);
        }

        public void RemoveTargetAttribute(CharacterAttribute c)
        {
            _targetAttributes.Remove(c);
        }

        private string ModifierToString(float modifier)
        {
            string modifierString = "";
            if (modifier != 0) modifierString = Helper.AddSignPrefix(modifier);
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
    }
}