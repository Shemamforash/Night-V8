using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Characters
{
    public class CharacterConditions
    {
        public readonly Dictionary<ConditionType, Condition> Conditions = new Dictionary<ConditionType, Condition>();
        public readonly ValueTextLink<string> Thoughts = new ValueTextLink<string>();

        public CharacterConditions()
        {
            Start();
        }
        
        private void Start()
        {
            Helper.ConstructObjectsFromCsv("Conditions", strings =>
            {
                ConditionType conditionType = StringToConditionType(strings[0]);
                string[] conditionStrings = {strings[1], strings[2], strings[3], strings[4], strings[5]};
                if (conditionType != ConditionType.Unknown)
                {
                    Conditions[conditionType] = new Condition(this, conditionStrings);
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

        public void UpdateCharacterThoughts()
        {
            string thoughtString = "";
            foreach (Condition condition in Conditions.Values)
            {
                string conditionString = condition.GetConditionString();
                if (conditionString != "")
                {
                    thoughtString += condition.GetConditionString() + " ";    
                }
            }
            if (thoughtString == "")
            {
                thoughtString = "Need to do something.";
            }
            Thoughts.Value(thoughtString);
        }
    }
}