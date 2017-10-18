using System;
using System.Collections.Generic;
using System.Linq;
using Game.World;
using SamsHelper;
using UnityEngine;

namespace Game.Characters
{
    public class Condition
    {
        private readonly Dictionary<Intensity, string> _intensityStrings = new Dictionary<Intensity, string>();
        private Intensity _intensity;
        private readonly CharacterConditions _conditions;
        private event Action OnIntensity;

        public Condition(CharacterConditions conditions, string[] strings)
        {
            _conditions = conditions;
            SetStrings(strings);
            WorldState.RegisterMinuteEvent(OnIntensity);
            SetConditionLevel(Intensity.None);
        }

        public void AddOnIntensity(Intensity intensity, Action action)
        {
            OnIntensity += delegate
            {
                if (_intensity == intensity)
                {
                    action();
                }
            };
        }
        
        public void SetConditionLevel(Intensity intensity)
        {
            _intensity = intensity;
            _conditions.UpdateCharacterThoughts();
        }

        public string GetConditionString()
        {
            Helper.PrintList(_intensityStrings.Values.ToList());
            return _intensityStrings[_intensity];
        }

        private void SetStrings(string[] strings)
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