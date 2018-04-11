using Game.Characters;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class UIConditionController : MonoBehaviour
    {
        public Slider ConditionSlider;
        public EnhancedText ConditionText;

        public void UpdateDehydration(Player player)
        {
            ConditionText.Text(player.Attributes.GetThirstStatus());
            if (ConditionSlider == null) return;
            ConditionSlider.value = 1 - player.Attributes.Dehydration.Normalised();
        }

        public void UpdateHunger(Player player)
        {
            ConditionText.Text(player.Attributes.GetHungerStatus());
            if (ConditionSlider == null) return;
            ConditionSlider.value = 1 - player.Attributes.Starvation.Normalised();
        }
    }
}