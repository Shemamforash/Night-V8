using System;
using Game.Characters;
using Game.Exploration.Environment;
using Game.Exploration.Weather;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class UIConditionController : MonoBehaviour
    {
        private Slider ConditionSlider;
        private EnhancedText ConditionText, ModifierIndicator;

        public void Awake()
        {
            if (transform.parent.parent.name == "Simple")
            {
                ConditionText = GetComponent<EnhancedText>();
                return;
            }
            ConditionText = Helper.FindChildWithName<EnhancedText>(gameObject, "Text");
            ConditionSlider = Helper.FindChildWithName<Slider>(gameObject, "Progress");
            ModifierIndicator = Helper.FindChildWithName<EnhancedText>(gameObject, "Modifier Indicator");
        }
        
        public void UpdateThirst(Player player)
        {
            ConditionText.Text(player.Attributes.GetThirstStatus());
            if (ConditionSlider == null) return;
            ConditionSlider.value = 1 - player.Attributes.Get(AttributeType.Thirst).Normalised();
            switch (EnvironmentManager.GetTemperature())
            {
                case TemperatureCategory.Freezing:
                    ModifierIndicator.Text("++");
                    break;
                case TemperatureCategory.Cold:
                    ModifierIndicator.Text("+");
                    break;
                case TemperatureCategory.Warm:
                    ModifierIndicator.Text("");
                    break;
                case TemperatureCategory.Hot:
                    ModifierIndicator.Text("-");
                    break;
                case TemperatureCategory.Boiling:
                    ModifierIndicator.Text("--");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void UpdateHunger(Player player)
        {
            ConditionText.Text(player.Attributes.GetHungerStatus());
            if (ConditionSlider == null) return;
            ConditionSlider.value = 1 - player.Attributes.Get(AttributeType.Hunger).Normalised();
            switch (EnvironmentManager.GetTemperature())
            {
                case TemperatureCategory.Freezing:
                    ModifierIndicator.Text("--");
                    break;
                case TemperatureCategory.Cold:
                    ModifierIndicator.Text("-");
                    break;
                case TemperatureCategory.Warm:
                    ModifierIndicator.Text("");
                    break;
                case TemperatureCategory.Hot:
                    ModifierIndicator.Text("+");
                    break;
                case TemperatureCategory.Boiling:
                    ModifierIndicator.Text("++");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}