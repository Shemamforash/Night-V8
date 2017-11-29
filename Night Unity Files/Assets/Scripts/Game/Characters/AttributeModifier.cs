using System.Collections.Generic;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Characters
{
    public class AttributeModifier
    {
        private float _summativeModifier;
        private float _multiplicativeModifier;
        private readonly List<CharacterAttribute> _targetAttributes= new List<CharacterAttribute>();
        public AttributeType AttributeType;
        private bool _applied;

        public void SetSummative(float summativeModifer)
        {
            Remove();
            _summativeModifier = summativeModifer;
        }
        
        public void SetMultiplicative(float multiplicativeModifier)
        {
            Remove();
            _multiplicativeModifier = multiplicativeModifier;
        }

        public void AddTargetAttributes(List<CharacterAttribute> targetAttributes)
        {
             _targetAttributes.AddRange(targetAttributes);   
        }
        
        public void AddTargetAttribute(CharacterAttribute targetAttribute)
        {
            _targetAttributes.Add(targetAttribute);
        }

        public void RemoveTargetAttribute(CharacterAttribute targetAttribute)
        {
            _targetAttributes.Remove(targetAttribute);
            _applied = false;
        }

        public void Apply()
        {
            if (_targetAttributes == null || _applied) return;
            _targetAttributes.ForEach(a => a.ApplySummativeModifier(_summativeModifier));
            _targetAttributes.ForEach(a => a.ApplyMultiplicativeModifier(_multiplicativeModifier));
            _applied = true;
        }

        public void Remove()
        {
            if (_targetAttributes == null || !_applied) return;
            _targetAttributes.ForEach(a => a.RemoveSummativeModifier(_summativeModifier));
            _targetAttributes.ForEach(a => a.RemoveMultiplicativeModifier(_multiplicativeModifier));
            _applied = false;
        }

        private string ModifierToString(float modifier)
        {
            string modifierString = "";
            if (modifier != 0)
            {
                modifierString = Helper.AddSignPrefix(modifier);
            }
            return modifierString;
        }
        
        public string SummativeModifierString()
        {
            string summativeModifierString = ModifierToString(_summativeModifier);
            if (summativeModifierString == "") return summativeModifierString;
            return summativeModifierString +  " " +  AttributeType;
        }

        public string MultiplicativeModifierString()
        {
            string multiplicativeModifierString = ModifierToString(_multiplicativeModifier * 100);
            if (multiplicativeModifierString == "") return multiplicativeModifierString;
            return multiplicativeModifierString +  "% " + AttributeType;
        }
    }
}