using Game.Gear.Weapons;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UIPlayerWeaponController : MonoBehaviour
    {
        private EnhancedText _nameText, _typeText, _dpsText, _qualityText;
        private GameObject _notEquippedObject, _equippedObject;
        public EnhancedButton EnhancedButton;

        public void Awake()
        {
            EnhancedButton = GetComponent<EnhancedButton>();
            _notEquippedObject = gameObject.FindChildWithName("Not Equipped");
            _equippedObject = gameObject.FindChildWithName("Equipped");
            _nameText = gameObject.FindChildWithName<EnhancedText>("Name");
            _typeText = gameObject.FindChildWithName<EnhancedText>("Type");
            _dpsText = gameObject.FindChildWithName<EnhancedText>("Dps");
            _qualityText = gameObject.FindChildWithName<EnhancedText>("Quality");
        }

        public void SetWeapon(Weapon weapon)
        {
            if (weapon == null)
            {
                _equippedObject.SetActive(false);
                _notEquippedObject.SetActive(true);
            }
            else
            {
                _notEquippedObject.SetActive(false);
                _equippedObject.SetActive(true);
                _nameText.SetText(weapon.Name);
                _typeText.SetText(weapon.GetWeaponType());
                _dpsText.SetText(weapon.WeaponAttributes.DPS().Round(1) + " DPS");
                _qualityText.SetText(weapon.Quality().ToString());
            }
        }
    }
}