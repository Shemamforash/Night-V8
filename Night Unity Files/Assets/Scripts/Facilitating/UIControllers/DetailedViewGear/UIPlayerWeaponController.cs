using Game.Gear.Weapons;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UIPlayerWeaponController : MonoBehaviour
    {
        private EnhancedText _nameText;
        private GameObject _notEquippedObject, _equippedObject;
        public EnhancedButton EnhancedButton;

        public void Awake()
        {
            EnhancedButton = GetComponent<EnhancedButton>();
            _notEquippedObject = gameObject.FindChildWithName("Not Equipped");
            _equippedObject = gameObject.FindChildWithName("Equipped");
            _nameText = _equippedObject.GetComponent<EnhancedText>();
        }

        public void SetWeapon(Weapon weapon)
        {
            bool isWeaponEquipped = weapon != null;
            _equippedObject.SetActive(isWeaponEquipped);
            _notEquippedObject.SetActive(!isWeaponEquipped);
            _nameText.SetText(isWeaponEquipped ? weapon.Name : "");
        }
    }
}