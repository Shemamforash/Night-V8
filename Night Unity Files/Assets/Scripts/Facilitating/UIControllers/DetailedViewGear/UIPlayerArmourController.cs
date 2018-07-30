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
            _equippedObject = Helper.FindChildWithName(gameObject, "Equipped");
            _notEquippedObject = Helper.FindChildWithName(gameObject, "Not Equipped");
            for (int i = 9; i >= 0; --i)
            {
                _plates.Add(Helper.FindChildWithName<Image>(gameObject, "Plate " + i));
            }

            _ratingText = Helper.FindChildWithName<EnhancedText>(gameObject, "Rating");
            _platesText = Helper.FindChildWithName<EnhancedText>(gameObject, "Plates");
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

            _ratingText.Text("Max Armour: " + armour.GetMaxArmour());
            _platesText.Text(armour.GetProtectionLevel() + " Armour");
        }
    }
}