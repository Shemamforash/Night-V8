using System.Collections.Generic;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Characters
{
    public class AttributeModifier
    {
        private readonly List<AttributeType> _targetAttributes = new List<AttributeType>();
        private AttributeContainer _lastAppliedContainer;
        private float _multMod = 1;
        private float _sumMod;

        public AttributeModifier(AttributeType attributeType)
        {
            _targetAttributes.Add(attributeType);
        }

        public AttributeModifier(List<AttributeType> attributeTypes)
        {
            _targetAttributes = attributeTypes;
        }

        public void SetSummative(float summativeModifer)
        {
            _sumMod = summativeModifer;
        }

        public void SetMultiplicative(float multiplicativeModifier)
        {
            _multMod = multiplicativeModifier;
        }

        public void ApplyOnce(AttributeContainer attributeContainer)
        {
            ApplyModifiers(attributeContainer);
        }

        private void ApplyModifiers(AttributeContainer attributeContainer)
        {
            foreach (AttributeType attribute in attributeContainer.Attributes.Keys)
            {
                if (!_targetAttributes.Contains(attribute)) continue;
                attributeContainer.Attributes[attribute].ApplyAddMod(_sumMod);
                attributeContainer.Attributes[attribute].ApplyMultMod(_multMod);
            }
        }

        public void Apply(AttributeContainer attributeContainer)
        {
            if (_lastAppliedContainer != null) Debug.Log("Did you mean to remove the modifiers from the last container?");
            ApplyModifiers(attributeContainer);
            _lastAppliedContainer = attributeContainer;
        }

        public void Remove()
        {
            if (_lastAppliedContainer == null) return;
            foreach (AttributeType attribute in _lastAppliedContainer.Attributes.Keys)
            {
                if (!_targetAttributes.Contains(attribute)) continue;
                _lastAppliedContainer.Attributes[attribute].RemoveAddMod(_sumMod);
                _lastAppliedContainer.Attributes[attribute].RemoveMultMod(_multMod);
            }

            _lastAppliedContainer = null;
        }

        private string ModifierToString(float modifier)
        {
            string modifierString = "";
            if (modifier != 0) modifierString = Helper.AddSignPrefix(modifier);

            return modifierString;
        }

        public string AddModString()
        {
            string addModString = ModifierToString(_sumMod);
            if (addModString == "") return addModString;
            return addModString;
        }

        public string MultModString()
        {
            if (_multMod == 1) return "";
            string multModString = ModifierToString((_multMod - 1) * 100);
            if (multModString == "") return multModString;
            return multModString;
        }
    }
}