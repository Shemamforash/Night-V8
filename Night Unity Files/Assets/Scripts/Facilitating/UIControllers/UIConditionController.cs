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
                case TemperatureCategory.Hot:
                    ModifierIndicator.SetText("+Thirst");
                    break;
                case TemperatureCategory.Burning:
                    ModifierIndicator.SetText("++Thirst");
                    break;
                default:
                    ModifierIndicator.SetText("");
                    break;
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
                    ModifierIndicator.SetText("++Hunger");
                    break;
                case TemperatureCategory.Cold:
                    ModifierIndicator.SetText("+Hunger");
                    break;
                default:
                    ModifierIndicator.SetText("");
                    break;
            }
        }
    }
}