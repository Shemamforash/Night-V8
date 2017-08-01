using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using World;

namespace Characters
{
    public class Character
    {
        private string name;
        private float strength, strengthMax, intelligence, intelligenceMax, endurance, stability;
        private float starvationTolerance, dehydrationTolerance, starvation, dehydration;
        private WeightCategory weight;
        private float sight;
        private Traits.Trait primaryTrait, secondaryTrait;

        private CharacterClass characterClass;
        private static Dictionary<string, CharacterClass> characterClasses = new Dictionary<string, CharacterClass>();
        public enum WeightCategory { LIGHT, MEDIUM, HEAVY };


        public Character(string name, string className, float strength, float intelligence, float endurance, float stability, float starvationTolerance, float dehydrationTolerance,
        WeightCategory weight, float sight, Traits.Trait primaryTrait, Traits.Trait secondaryTrait)
        {
            this.name = name;
            this.characterClass = characterClasses[className];
            this.strength = strength;
            this.strengthMax = strength;
            this.intelligence = intelligence;
            this.intelligenceMax = intelligence;
            this.endurance = endurance;
            this.stability = stability;
            this.starvationTolerance = starvationTolerance;
            this.dehydrationTolerance = dehydrationTolerance;
            this.weight = weight;
            this.sight = sight;
            this.primaryTrait = primaryTrait;
            this.secondaryTrait = secondaryTrait;
        }

        private class CharacterClass
        {
            public enum CharacterClassType { };
            private CharacterClassType classType;
            private string className;

            public CharacterClass(CharacterClassType classType, string className)
            {
                this.classType = classType;
                this.className = className;
            }

            public CharacterClassType ClassType()
            {
                return classType;
            }

            public string ClassName()
            {
                return className;
            }
        }
    }
}