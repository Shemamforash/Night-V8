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
        public bool Simple;

        public void Awake()
        {
            if (transform.parent.parent.name == "Simple")
            {
                ConditionText = GetComponent<EnhancedText>();
                return;
            }
            ConditionText = gameObject.FindChildWithName<EnhancedText>("Text");
            ConditionSlider = gameObject.FindChildWithName<Slider>("Progress");
            if (Simple) return;
            ModifierIndicator = gameObject.FindChildWithName<EnhancedText>("Modifier Indicator");
        }
        
        public void UpdateThirst(Player player)
        {
            ConditionText.SetText(player.Attributes.GetThirstStatus());
            if (ConditionSlider == null) return;
            ConditionSlider.value = 1 - player.Attributes.Get(AttributeType.Thirst).Normalised();
            if (Simple) return;
            switch (EnvironmentManager.GetTemperature())
            {
                case TemperatureCategory.Freezing:
                    ModifierIndicator.SetText("Retaining lots of water");
                    break;
                case TemperatureCategory.Cold:
                    ModifierIndicator.SetText("Retaining some water");
                    break;
                case TemperatureCategory.Warm:
                    ModifierIndicator.SetText("Feeling normal");
                    break;
                case TemperatureCategory.Hot:
                    ModifierIndicator.SetText("Sweating slightly");
                    break;
                case TemperatureCategory.Boiling:
                    ModifierIndicator.SetText("Sweating heavily");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void UpdateHunger(Player player)
        {
            ConditionText.SetText(player.Attributes.GetHungerStatus());
            if (ConditionSlider == null) return;
            ConditionSlider.value = 1 - player.Attributes.Get(AttributeType.Hunger).Normalised();
            if (Simple) return;
            switch (EnvironmentManager.GetTemperature())
            {
                case TemperatureCategory.Freezing:
                    ModifierIndicator.SetText("Losing lots of heat");
                    break;
                case TemperatureCategory.Cold:
                    ModifierIndicator.SetText("Losing some heat");
                    break;
                case TemperatureCategory.Warm:
                    ModifierIndicator.SetText("Feeling normal");
                    break;
                case TemperatureCategory.Hot:
                    ModifierIndicator.SetText("Barely losing any heat");
                    break;
                case TemperatureCategory.Boiling:
                    ModifierIndicator.SetText("Losing no heat");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}