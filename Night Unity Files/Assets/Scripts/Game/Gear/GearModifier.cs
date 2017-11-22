using System.Collections.Generic;
using Game.Characters;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public class GearModifier
    {
        public readonly string Name;
        private List<AttributeModifier> _modifiers = new List<AttributeModifier>();

        public GearModifier(string name)
        {
            Name = name;
        }

        public virtual string GetDescription()
        {
            string description = Name + ":";
            foreach (AttributeModifier modifier in _modifiers)
            {
                string summativeString = modifier.SummativeModifierString();
                if (summativeString != "") description += "\n" + summativeString;

                string multiplicativeString = modifier.MultiplicativeModifierString();
                if (multiplicativeString != "") description += "\n" + multiplicativeString;
            }
            return description;
        }

        public void AddAttributeModifier(AttributeModifier modifier)
        {
            _modifiers.Add(modifier);
        }

        public void CreateAndAddAttributeModifier(CharacterAttribute target, float multiplicativeMultiplier, float summativeMultiplier)
        {
            AttributeModifier modifier = new AttributeModifier();
            modifier.SetMultiplicative(multiplicativeMultiplier);
            modifier.SetSummative(summativeMultiplier);
            modifier.AddTargetAttribute(target);
            _modifiers.Add(modifier);
        }

        public void Apply()
        {
            _modifiers.ForEach(m => m.Apply());
        }

        public void Remove()
        {
            _modifiers.ForEach(m => m.Remove());
        }

        public void SetTarget(AttributeContainer container)
        {
            foreach (AttributeModifier modifier in _modifiers)
            {
                CharacterAttribute attribute = container.Get(modifier.AttributeType);
                if (attribute != null)
                {
                    modifier.AddTargetAttribute(attribute);
                }
            }
        }

        public void ApplyToGear(AttributeContainer container)
        {
            SetTarget(container);
            Apply();
            RemoveTarget();
        }

        public void RemoveTarget()
        {
            foreach (AttributeModifier modifier in _modifiers)
            {
                modifier.AddTargetAttribute(null);
            }
        }
    }
}