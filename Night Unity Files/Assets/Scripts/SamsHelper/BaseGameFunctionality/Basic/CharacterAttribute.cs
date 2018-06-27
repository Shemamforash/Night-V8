using System.Collections.Generic;
using Game.Characters;
using NUnit.Framework;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public class CharacterAttribute : Number
    {
        public readonly AttributeType AttributeType;
        private float _rawBonus;
        private float _finalBonus;
        private readonly List<AttributeModifier> _modifiers = new List<AttributeModifier>();
        private float _calculatedValue;

        public CharacterAttribute(AttributeType attributeType)
        {
            AttributeType = attributeType;
        }

        public override float CurrentValue()
        {
            return Mathf.Clamp(_calculatedValue, Min, Max);
        }

        public override void Increment(float amount = 1)
        {
            base.Increment(amount);
            Recalculate();
        }

        public override void Decrement(float amount = 1)
        {
            base.Decrement(amount);
            Recalculate();
        }

        public override void SetCurrentValue(float value)
        {
            base.SetCurrentValue(value);
            Recalculate();
        }

        private void CalculateFinalValue()
        {
            _calculatedValue = (base.CurrentValue() + _rawBonus) * _finalBonus;
        }

        public void Recalculate()
        {
            _rawBonus = 0;
            _finalBonus = 1;

            _modifiers.ForEach(m =>
            {
                _rawBonus += m.RawBonus();
                _finalBonus += m.FinalBonus();
            });
            CalculateFinalValue();
        }

        public void AddModifier(AttributeModifier modifier)
        {
            Assert.IsFalse(_modifiers.Contains(modifier));
            _modifiers.Add(modifier);
            modifier.AddTargetAttribute(this);
            Recalculate();
        }

        public void RemoveModifier(AttributeModifier modifier)
        {
            _modifiers.Remove(modifier);
            modifier.RemoveTargetAttribute(this);
            Recalculate();
        }

        public void SetToMax()
        {
            SetCurrentValue(Max);
        }
    }
}