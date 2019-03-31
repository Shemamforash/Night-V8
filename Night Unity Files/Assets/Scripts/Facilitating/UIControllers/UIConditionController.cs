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
        private Image _fillImage, _offsetImage;

        public void Awake()
        {
            if (transform.parent.parent.name == "Simple") return;
            _offsetImage = gameObject.FindChildWithName<Image>("Offset");
            _fillImage = gameObject.FindChildWithName<Image>("Fill");
        }

        public void UpdateThirst(Player player, float offset = 0)
        {
            if (_fillImage == null) return;
            UpdateSlider(AttributeType.Thirst, player, offset);
        }

        private void UpdateSlider(AttributeType attributeType, Player player, float offset)
        {
            float current = player.Attributes.Val(attributeType);
            float max = player.Attributes.Max(attributeType);
            if (current + offset > max) offset -= current + offset - max;
            _fillImage.fillAmount = 1 - current / max;
            _offsetImage.fillAmount = offset == 0 ? 0 : 1 - (current + offset) / max;
        }

        public void UpdateHunger(Player player, float offset = 0)
        {
            if (_fillImage == null) return;
            UpdateSlider(AttributeType.Hunger, player, offset);
        }
    }
}