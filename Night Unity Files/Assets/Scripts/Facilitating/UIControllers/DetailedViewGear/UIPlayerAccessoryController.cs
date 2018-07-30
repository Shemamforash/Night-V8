using Game.Gear.Armour;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UIPlayerAccessoryController : MonoBehaviour
    {
        private EnhancedText _accessoryText;
        private GameObject _notEquippedObject;
        public EnhancedButton EnhancedButton;

        public void Awake()
        {
            EnhancedButton = GetComponent<EnhancedButton>();
            _accessoryText = Helper.FindChildWithName<EnhancedText>(gameObject, "Equipped");
            _notEquippedObject = Helper.FindChildWithName(gameObject, "Not Equipped");
            SetAccessory(null);
        }

        public void SetAccessory(Accessory accessory)
        {
            if (accessory == null)
            {
                _notEquippedObject.SetActive(true);
                _accessoryText.gameObject.SetActive(false);
            }
            else
            {
                _notEquippedObject.SetActive(false);
                _accessoryText.gameObject.SetActive(true);
                _accessoryText.Text(accessory.Name);
            }
        }
    }
}