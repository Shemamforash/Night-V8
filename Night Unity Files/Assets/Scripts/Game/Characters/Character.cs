using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using World;

namespace Characters
{
    public class Character
    {
        public GameObject characterUI;
        private string name;
        private float strength, strengthMax, intelligence, intelligenceMax, endurance, stability;
        private float starvationTolerance, dehydrationTolerance, starvation, dehydration;
        private WeightCategory weight;
        private float sight;
        private Traits.Trait primaryTrait, secondaryTrait;

        private CharacterClass characterClass;
        public enum WeightCategory { LIGHT, MEDIUM, HEAVY };

        public Character(string name, string className, float strength, float intelligence, float endurance, float stability, float starvationTolerance, float dehydrationTolerance,
        WeightCategory weight, float sight, string secondaryTrait)
        {
            this.name = name;
            this.characterClass = CharacterClass.FindClass(className);
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
            this.primaryTrait = Traits.FindTrait(characterClass.ClassTrait());
            this.secondaryTrait = Traits.FindTrait(secondaryTrait);
        }
    }
}