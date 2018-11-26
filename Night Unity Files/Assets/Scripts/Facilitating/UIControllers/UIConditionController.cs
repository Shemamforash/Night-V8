using Game.Characters;
using Game.Exploration.Environment;
using Game.Exploration.Weather;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class UIConditionController : MonoBehaviour
    {
        private Slider ConditionSlider;
        private EnhancedText ConditionText;

        public void Awake()
        {
            if (transform.parent.parent.name == "Simple")
            {
                ConditionText = GetComponent<EnhancedText>();
                return;
            }

            ConditionText = gameObject.FindChildWithName<EnhancedText>("Text");
            ConditionSlider = gameObject.FindChildWithName<Slider>("Progress");
        }

        public void UpdateThirst(Player player, float offset = 0)
        {
            ConditionText.SetText(player.Attributes.GetThirstStatus());
            if (ConditionSlider == null) return;
            UpdateSlider(AttributeType.Thirst, player, offset);
        }

        private void UpdateSlider(AttributeType attributeType, Player player, float offset)
        {
            float currentValue = player.Attributes.Val(attributeType) + offset;
            float maxValue = player.Attributes.Max(attributeType);
            currentValue = Mathf.Clamp(currentValue, 0, maxValue);
            currentValue /= maxValue;
            ConditionSlider.value = 1 - currentValue;
        }

        public void UpdateHunger(Player player, float offset = 0)
        {
            ConditionText.SetText(player.Attributes.GetHungerStatus());
            if (ConditionSlider == null) return;
            UpdateSlider(AttributeType.Hunger, player, offset);
        }
    }
}