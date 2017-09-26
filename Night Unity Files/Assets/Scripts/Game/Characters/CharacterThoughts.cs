using System;
using System.Collections.Generic;
using System.Linq;
using SamsHelper;
using UnityEngine;

namespace Game.Characters
{
    public class CharacterThoughts
    {
        private readonly Dictionary<Condition, Thought> _thoughtDictionary = new Dictionary<Condition, Thought>();
        private readonly Dictionary<Condition, string> _thoughts = new Dictionary<Condition, string>();

        public void Start()
        {
            foreach (Condition c in Enum.GetValues(typeof(Condition)))
            {
                _thoughtDictionary[c] = new Thought(c);
                _thoughts[c] = "";
            }
            Helper.ConstructObjectsFromCsv("Conditions", strings =>
            {
                string conditionName = strings[0];
                string[] conditions = {strings[1], strings[2], strings[3], strings[4], strings[5]};
                switch (conditionName)
                {
                    case "Hunger":
                        _thoughtDictionary[Condition.Hunger].SetStrings(conditions);
                        break;
                    case "Thirst":
                        _thoughtDictionary[Condition.Thirst].SetStrings(conditions);
                        break;
                    default:
                        Debug.Log("Unknown condition " + conditionName);
                        break;
                }
            });
        }

        public void GenerateThought(Condition t, Intensity intensity)
        {
            _thoughts[t] = _thoughtDictionary[t].GetThought(intensity);
        }

        private class Thought
        {
            private readonly Condition Type;
            private readonly Dictionary<Intensity, string> _intensityStrings = new Dictionary<Intensity, string>();

            public Thought(Condition type)
            {
                Type = type;
            }

            public string GetThought(Intensity intensity)
            {
                return _intensityStrings[intensity];
            }

            public void SetStrings(string[] strings)
            {
                _intensityStrings[Intensity.None] = "";
                _intensityStrings[Intensity.Slight] = strings[0];
                _intensityStrings[Intensity.Mild] = strings[1];
                _intensityStrings[Intensity.Medium] = strings[2];
                _intensityStrings[Intensity.Strong] = strings[3];
                _intensityStrings[Intensity.Unbearable] = strings[4];
            }
        }
    }
}