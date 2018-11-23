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

        public void UpdateThirst(Player player)
        {
            ConditionText.SetText(player.Attributes.GetThirstStatus());
            if (ConditionSlider == null) return;
            ConditionSlider.value = 1 - player.Attributes.Get(AttributeType.Thirst).Normalised();
        }

        public void UpdateHunger(Player player)
        {
            ConditionText.SetText(player.Attributes.GetHungerStatus());
            if (ConditionSlider == null) return;
            ConditionSlider.value = 1 - player.Attributes.Get(AttributeType.Hunger).Normalised();
        }
    }
}