using System.Collections.Generic;
using Game.Gear.Armour;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class UIPlayerArmourController : MonoBehaviour
    {
        private readonly List<Image> _plates = new List<Image>();
        private GameObject _equippedObject, _notEquippedObject;
        public EnhancedButton EnhancedButton;

        public void Awake()
        {
            EnhancedButton = GetComponent<EnhancedButton>();
            _equippedObject = gameObject.FindChildWithName("Armour Bar");
            _notEquippedObject = gameObject.FindChildWithName("Not Equipped");
            for (int i = 9; i >= 0; --i)
            {
                _plates.Add(gameObject.FindChildWithName<Image>("Plate " + i));
            }
        }

        public void SetArmour(ArmourController armour)
        {
            if (_notEquippedObject == null) return;
            bool hasProtection = armour.GetTotalProtection() != 0;
            _notEquippedObject.SetActive(!hasProtection);
            _equippedObject.SetActive(hasProtection);
            if (!hasProtection) return;
            for (int i = 0; i < _plates.Count; ++i) _plates[i].color = i >= armour.GetTotalProtection() ? UiAppearanceController.InvisibleColour : Color.white;
        }
    }
}