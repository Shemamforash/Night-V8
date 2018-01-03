﻿using System;
using System.Collections.Generic;
using System.Xml;
using SamsHelper;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Characters
{
    public static class TraitLoader
    {
        private static readonly Dictionary<string, Trait> TraitDictionary = new Dictionary<string, Trait>();
        private static readonly Dictionary<string, Trait> ClassDictionary = new Dictionary<string, Trait>();
        private static readonly List<string> TraitNames = new List<string>();
        private static readonly List<string> ClassNames = new List<string>();

        public static Trait GenerateTrait()
        {
            return TraitDictionary[TraitNames[Random.Range(0, TraitNames.Count)]];
        }

        public static Trait GenerateClass()
        {
            return ClassDictionary[ClassNames[Random.Range(0, ClassNames.Count)]];
        }

        private static string DefaultValueIfEmpty(string[] arr, int position, string defaultString)
        {
            return arr[position] == "" ? defaultString : arr[position];
        }

        public static void LoadTraits()
        {
            string traitText = Resources.Load<TextAsset>("Traits").text;
            XmlDocument traitXml = new XmlDocument();
            traitXml.LoadXml(traitText);
            XmlNode root = traitXml.SelectSingleNode("Traits");
            foreach (XmlNode classNode in root.SelectNodes("Class"))
            {
                string name = classNode.SelectSingleNode("Name").InnerText;
                int endurance = int.Parse(classNode.SelectSingleNode("Endurance").InnerText);
                int willpower = int.Parse(classNode.SelectSingleNode("Willpower").InnerText);
                int strength = int.Parse(classNode.SelectSingleNode("Strength").InnerText);
                int perception = int.Parse(classNode.SelectSingleNode("Perception").InnerText);
                int weight = int.Parse(classNode.SelectSingleNode("Weight").InnerText);
                Trait newClass = new Trait(name, strength, endurance, willpower, perception, weight);
                ClassDictionary[name] = newClass;
                ClassNames.Add(name);
            }
            foreach (XmlNode traitNode in root.SelectNodes("Trait"))
            {
                //TODO strength and perception import
                string name = traitNode.SelectSingleNode("Name").InnerText;
                int endurance = int.Parse(traitNode.SelectSingleNode("Endurance").InnerText);
                int willpower = int.Parse(traitNode.SelectSingleNode("Willpower").InnerText);
                int weight = int.Parse(traitNode.SelectSingleNode("Weight").InnerText);
                Trait newTrait = new Trait(name, 0, endurance, willpower, 0, weight);
                TraitDictionary[name] = newTrait;
                TraitNames.Add(name);
            }
        }

        public static Trait FindTrait(string traitName)
        {
            try
            {
                return TraitDictionary[traitName];
            }
            catch (KeyNotFoundException)
            {
                throw new Exceptions.UnknownTraitException(traitName);
            }
        }

        public static Trait FindClass(string className)
        {
            try
            {
                return ClassDictionary[className];
            }
            catch (KeyNotFoundException)
            {
                throw new Exceptions.UnknownTraitException(className);
            }
        }

        public class Trait
        {
            public string Name;

            public int Strength, Endurance, Willpower, Perception, Weight;

            public Trait(string name, int strength, int endurance, int willpower, int perception, int weight)
            {
                Name = name;
                Strength = strength;
                Endurance = endurance;
                Willpower = willpower;
                Perception = perception;
                Weight = weight;
            }

            public virtual string GetTraitDetails()
            {
                string traitDetails = Name + ":";
                traitDetails += GetValueAsString(Willpower, " stb");
                traitDetails += GetValueAsString(Endurance, " end");
                return traitDetails;
            }

            private static string GetValueAsString(int value, string suffix)
            {
                string roundedValue = value.ToString();
                if (value == 0)
                {
                    return "";
                }
                if (value > 0)
                {
                    roundedValue = "+" + roundedValue;
                }
                return "\n    " + roundedValue + " " + suffix;
            }
        }
    }
}