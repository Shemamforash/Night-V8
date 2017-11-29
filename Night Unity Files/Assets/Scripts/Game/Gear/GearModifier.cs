using System;
using System.Collections.Generic;
using System.Linq;
using Game.Characters;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public class GearModifier
    {
        public readonly string Name;
        private readonly List<AttributeModifier> _modifiers = new List<AttributeModifier>();

        public GearModifier(string name)
        {
            Name = name;
        }

        public string ModifierString()
        {
            return _modifiers.Aggregate("", (current, modifier) => current + (modifier.SummativeModifierString() == "" ? modifier.MultiplicativeModifierString() : modifier.SummativeModifierString()));
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

        public void ApplyToGear(AttributeContainer container)
        {
            foreach (AttributeModifier modifier in _modifiers)
            {
                CharacterAttribute attribute = container.Get(modifier.AttributeType);
                if (attribute == null) continue;
                modifier.AddTargetAttribute(attribute);
                modifier.Apply();
                modifier.RemoveTargetAttribute(attribute);
            }
        }
    }
}