using System.Collections.Generic;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Characters
{
    public class AttributeModifier
    {
        private float _sumMod;
        private float _multMod = 1;
        private readonly List<CharacterAttribute> _targetAttributes= new List<CharacterAttribute>();
        public AttributeType AttributeType;
        private bool _applied;

        public void SetSummative(float summativeModifer)
        {
            Remove();
            _sumMod = summativeModifer;
        }
        
        public void SetMultiplicative(float multiplicativeModifier)
        {
            Remove();
            _multMod = multiplicativeModifier;
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
            _targetAttributes.ForEach(a => a.ApplyAddMod(_sumMod));
            _targetAttributes.ForEach(a => a.ApplyMultMod(_multMod));
            _applied = true;
        }

        public void Remove()
        {
            if (_targetAttributes == null || !_applied) return;
            _targetAttributes.ForEach(a => a.RemoveAddMod(_sumMod));
            _targetAttributes.ForEach(a => a.RemoveMultMod(_multMod));
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
        
        public string AddModString()
        {
            string addModString = ModifierToString(_sumMod);
            if (addModString == "") return addModString;
            return addModString +  " " +  AttributeType;
        }

        public string MultModString()
        {
            if (_multMod == 1) return "";
            string multModString = ModifierToString((_multMod - 1) * 100);
            if (multModString == "") return multModString;
            return multModString +  "% " + AttributeType;
        }
    }
}