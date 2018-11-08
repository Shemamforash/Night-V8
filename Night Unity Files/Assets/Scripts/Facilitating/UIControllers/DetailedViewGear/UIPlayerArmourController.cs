using Game.Gear.Armour;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class UIPlayerArmourController : MonoBehaviour
    {
        public EnhancedButton EnhancedButton;
        private UIArmourController _armourController;

        public void Awake()
        {
            _armourController = gameObject.FindChildWithName<UIArmourController>("Armour Bar"); 
            EnhancedButton = GetComponent<EnhancedButton>();
            EnhancedButton.AddOnClick(UiGearMenuController.ShowArmourMenu);
            GlowButtonBehaviour glow = GetComponent<GlowButtonBehaviour>();
            EnhancedButton.AddOnClick(glow.Select);
            EnhancedButton.AddOnDeselectEvent(glow.Deselect);
            EnhancedButton.AddOnSelectEvent(glow.Highlight);
        }

        public void SetArmour(ArmourController armour)
        {
            _armourController.TakeDamage(armour);
        }
    }
}