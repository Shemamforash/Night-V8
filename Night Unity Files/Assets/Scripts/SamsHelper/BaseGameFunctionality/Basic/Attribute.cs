using System;
using System.Collections.Generic;
using SamsHelper.ReactiveUI.CustomTypes;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public class Attribute : MyInt
    {
        public readonly AttributeType AttributeType;
        private List<AttributeModifier> _modifiers = new List<AttributeModifier>();
        private float _calculatedValue;
        
        public Attribute(AttributeType attributeType, int value, int min = 0, int max = 0) : base(value, min, max)
        {
            AttributeType = attributeType;
            _calculatedValue = Val;
        }

        public int CalculatedValue() => (int)Math.Round(_calculatedValue);

        public void AddModifier(AttributeModifier modifier)
        {
            _modifiers.Add(modifier);
            RecalculateModifiers();
        }

        public void RemoveModifier(AttributeModifier modifier)
        {
            _modifiers.Remove(modifier);
            RecalculateModifiers();
        }

        private void RecalculateModifiers()
        {
            float additiveModifier = 0;
            float multiplicativeModifier = 0;
            _modifiers.ForEach(m =>
            {
                switch (m.Type)
                {
                    case AttributeModifier.ModifierType.Additive:
                        m.ApplyModifier(ref additiveModifier);
                        break;
                    case AttributeModifier.ModifierType.Multiplicative:
                        m.ApplyModifier(ref multiplicativeModifier);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
            _calculatedValue = (Val + additiveModifier) * multiplicativeModifier;
        }
    }
}