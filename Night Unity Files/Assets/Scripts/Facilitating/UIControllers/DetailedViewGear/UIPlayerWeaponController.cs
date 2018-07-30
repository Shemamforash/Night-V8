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
            _notEquippedObject = Helper.FindChildWithName(gameObject, "Not Equipped");
            _equippedObject = Helper.FindChildWithName(gameObject, "Equipped");
            _nameText = Helper.FindChildWithName<EnhancedText>(gameObject, "Name");
            _typeText = Helper.FindChildWithName<EnhancedText>(gameObject, "Type");
            _dpsText = Helper.FindChildWithName<EnhancedText>(gameObject, "Dps");
            _qualityText = Helper.FindChildWithName<EnhancedText>(gameObject, "Quality");
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
                _nameText.Text(weapon.Name);
                _typeText.Text(weapon.GetWeaponType());
                _dpsText.Text(Helper.Round(weapon.WeaponAttributes.DPS(), 1) + " DPS");
                _qualityText.Text(weapon.Quality().ToString());
            }
        }
    }
}