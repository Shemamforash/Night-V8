using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Characters;
using UnityEngine;

namespace Game.Characters
{
    public class CharacterThoughts
    {
        private readonly Dictionary<ConditionType, Condition> _thoughtDictionary = new Dictionary<ConditionType, Condition>();
        private readonly Character _character;

        public CharacterThoughts(Character character)
        {
            _character = character;
            Start();
        }
        
        private void Start()
        {
            foreach (ConditionType c in Enum.GetValues(typeof(ConditionType)))
            {
                if (c != ConditionType.Unknown)
                {
                    _thoughtDictionary[c] = new Condition(this, c);
                }
            }
            Helper.ConstructObjectsFromCsv("Conditions", strings =>
            {
                ConditionType conditionType = StringToConditionType(strings[0]);
                string[] conditions = {strings[1], strings[2], strings[3], strings[4], strings[5]};
                if (conditionType != ConditionType.Unknown)
                {
                    _thoughtDictionary[conditionType].SetStrings(conditions);
                }
                else
                {
                    Debug.Log("Unknown condition type: " + conditionType);
                }
            });
        }

        private static ConditionType StringToConditionType(string s)
        {
            foreach (ConditionType c in Enum.GetValues(typeof(ConditionType)))
            {
                if (c.ToString() == s)
                {
                    return c;
                }
            }
            return ConditionType.Unknown;
        }

        private void UpdateCharacterThoughts()
        {
            string thoughtString = "";
            foreach (Condition condition in _thoughtDictionary.Values)
            {
                string conditionString = condition.GetConditionString();
                if (conditionString != "")
                {
                    thoughtString += condition.GetConditionString() + " ";    
                }
            }
            _character.CharacterUiDetailed.ConditionsText.text = thoughtString;
        }

        private class Condition
        {
            private readonly ConditionType _type;
            private readonly Dictionary<Intensity, string> _intensityStrings = new Dictionary<Intensity, string>();
            private Intensity _intensity;
            private readonly CharacterThoughts _thoughts;

            public Condition(CharacterThoughts thoughts, ConditionType type)
            {
                _type = type;
                _thoughts = thoughts;
            }

            public void SetConditionLevel(Intensity intensity)
            {
                _intensity = intensity;
                _thoughts.UpdateCharacterThoughts();
            }

            public string GetConditionString()
            {
                return _intensityStrings[_intensity];
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