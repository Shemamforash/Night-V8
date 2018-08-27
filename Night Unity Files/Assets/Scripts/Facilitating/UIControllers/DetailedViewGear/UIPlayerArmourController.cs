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
        private EnhancedText _ratingText, _platesText;
        public EnhancedButton EnhancedButton;

        public void Awake()
        {
            EnhancedButton = GetComponent<EnhancedButton>();
            _equippedObject = gameObject.FindChildWithName("Equipped");
            _notEquippedObject = gameObject.FindChildWithName("Not Equipped");
            for (int i = 9; i >= 0; --i)
            {
                _plates.Add(gameObject.FindChildWithName<Image>("Plate " + i));
            }

            _ratingText = gameObject.FindChildWithName<EnhancedText>("Rating");
            _platesText = gameObject.FindChildWithName<EnhancedText>("Plates");
        }

        public void SetArmour(ArmourController armour)
        {
            if (armour.GetProtectionLevel() == 0)
            {
                _notEquippedObject.SetActive(true);
                _equippedObject.SetActive(false);
                return;
            }

            _notEquippedObject.SetActive(false);
            _equippedObject.SetActive(true);
            for (int i = 0; i < _plates.Count; ++i) _plates[i].color = i >= armour.GetProtectionLevel() ? UiAppearanceController.InvisibleColour : Color.white;

            _ratingText.SetText("Max Armour: " + armour.GetMaxArmour());
            _platesText.SetText(armour.GetProtectionLevel() + " Armour");
        }
    }
}